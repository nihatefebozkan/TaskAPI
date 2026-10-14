using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TaskAPI.UI.Services
{
    public sealed class ApiClient(HttpClient http)
    {
        // Token'lar tarayıcı depolamasına yazılmaz. Tam sayfa yenilemesi oturumu temizler.
        private TokenResponse? tokens;
        private readonly SemaphoreSlim requestLock = new(1, 1);

        public string Username { get; private set; } = string.Empty;
        public bool IsSignedIn => tokens is not null;

        public async Task Login(string username, string password)
        {
            await requestLock.WaitAsync();
            try
            {
                using var response = await http.PostAsJsonAsync("api/Auth/Login", new { username, password });
                await EnsureSuccess(response);
                var result = await ReadTokens(response);
                tokens = result;
                Username = username;
            }
            finally
            {
                requestLock.Release();
            }
        }

        public async Task Register(string username, string password)
        {
            using var response = await http.PostAsJsonAsync("api/Auth/Register", new { username, password });
            await EnsureSuccess(response);
        }

        public async Task<List<TaskItem>> GetTasks()
        {
            using var response = await Send(() => new HttpRequestMessage(HttpMethod.Get, "api/Task"));
            return await response.Content.ReadFromJsonAsync<List<TaskItem>>() ?? [];
        }

        public async Task<TaskItem> GetTask(int id)
        {
            using var response = await Send(() => new HttpRequestMessage(HttpMethod.Get, $"api/Task/{id}"));
            return await response.Content.ReadFromJsonAsync<TaskItem>() ?? throw new InvalidOperationException("Görev bilgileri okunamadı.");
        }

        public async Task<TaskItem> CreateTask(string title, string description, DateTime dueDate)
        {
            using var response = await Send(() => new HttpRequestMessage(HttpMethod.Post, "api/Task")
            {
                Content = JsonContent.Create(new { title, description, dueDate })
            });
            return await response.Content.ReadFromJsonAsync<TaskItem>() ?? throw new InvalidOperationException("Görev cevabı okunamadı. Listeyi yenileyerek kontrol et.");
        }

        public async Task<TaskItem> UpdateTask(int id, DateTime dueDate)
        {
            using var response = await Send(() => new HttpRequestMessage(HttpMethod.Put, $"api/Task/{id}")
            {
                Content = JsonContent.Create(new { dueDate })
            });
            return await response.Content.ReadFromJsonAsync<TaskItem>() ?? throw new InvalidOperationException("Güncelleme cevabı okunamadı. Listeyi yenileyerek kontrol et.");
        }

        public async Task DeleteTask(int id)
        {
            using var response = await Send(() => new HttpRequestMessage(HttpMethod.Delete, $"api/Task/{id}"));
        }

        public async Task<bool> Logout()
        {
            await requestLock.WaitAsync();
            try
            {
                if (tokens is null)
                    return true;

                // API Refresh ve Logout endpoint'leri JSON string kabul ediyor.
                using var response = await http.PostAsJsonAsync("api/Auth/Logout", tokens.RefreshToken);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            finally
            {
                ClearSession();
                requestLock.Release();
            }
        }

        private async Task<HttpResponseMessage> Send(Func<HttpRequestMessage> createRequest)
        {
            // Korunan istekler sıraya alınır; aynı refresh token eşzamanlı tüketilmez.
            await requestLock.WaitAsync();
            try
            {
                if (tokens is null)
                    throw SessionExpired();

                if (tokens.ExpireAt <= DateTime.UtcNow.AddSeconds(15))
                    await Refresh();

                using var request = createRequest();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
                var response = await http.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    response.Dispose();
                    await Refresh();

                    using var retry = createRequest();
                    retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
                    response = await http.SendAsync(retry);

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        response.Dispose();
                        ClearSession();
                        throw SessionExpired();
                    }
                }

                try
                {
                    await EnsureSuccess(response);
                    return response;
                }
                catch
                {
                    response.Dispose();
                    throw;
                }
            }
            finally
            {
                requestLock.Release();
            }
        }

        private async Task Refresh()
        {
            if (tokens is null || tokens.RefreshTokenExpireAt <= DateTime.UtcNow)
            {
                ClearSession();
                throw SessionExpired();
            }

            using var response = await http.PostAsJsonAsync("api/Auth/Refresh", tokens.RefreshToken);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
            {
                ClearSession();
                throw SessionExpired();
            }

            await EnsureSuccess(response);
            tokens = await ReadTokens(response);
        }

        private static async Task<TokenResponse> ReadTokens(HttpResponseMessage response)
        {
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken) || string.IsNullOrWhiteSpace(result.RefreshToken) || result.ExpireAt <= DateTime.UtcNow || result.RefreshTokenExpireAt <= DateTime.UtcNow)
                throw new InvalidOperationException("Sunucudan geçerli oturum bilgisi alınamadı.");

            return result;
        }

        private void ClearSession()
        {
            tokens = null;
            Username = string.Empty;
        }

        private static HttpRequestException SessionExpired()
        {
            return new HttpRequestException("Oturumun sona erdi. Lütfen tekrar giriş yap.", null, HttpStatusCode.Unauthorized);
        }

        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            var message = response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Kullanıcı adı veya şifre hatalı.",
                HttpStatusCode.Forbidden => "Bu işlem için yetkin bulunmuyor.",
                HttpStatusCode.NotFound => "Görev bulunamadı veya bu göreve erişimin yok.",
                HttpStatusCode.Conflict => "Bu kullanıcı adı zaten alınmış.",
                HttpStatusCode.BadRequest => "Bilgileri kontrol edip tekrar dene.",
                _ => "İşlem tamamlanamadı. Lütfen biraz sonra tekrar dene."
            };

            // Ayrıntılı 500 mesajlarını arayüze taşımıyoruz.
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                try
                {
                    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (json.RootElement.TryGetProperty("error", out var error) && error.TryGetProperty("message", out var value))
                        message = value.GetString() ?? message;
                }
                catch (JsonException) { }
            }

            throw new HttpRequestException(message, null, response.StatusCode);
        }

        private sealed class TokenResponse
        {
            public TokenResponse() { }

            public string AccessToken { get; set; } = string.Empty;
            public DateTime ExpireAt { get; set; }
            public string RefreshToken { get; set; } = string.Empty;
            public DateTime RefreshTokenExpireAt { get; set; }
        }

        public sealed class TaskItem
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime DueDate { get; set; }
            public bool IsCompleted { get; set; }
        }
    }
}
