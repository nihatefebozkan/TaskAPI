using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using Xunit;
using User = TaskAPI.Domain.Entity.User;

namespace TaskAPI.API.Tests
{
    public class AuthenticationTests
    {
        private const string Issuer = "TaskAPI.Tests";
        private const string Audience = "TaskAPI.Tests";
        private const string Username = "test-user";
        private const string Password = "test-password";

        // 64 byte: Yanlış algoritma testinde HS384 de üretebilmek için.
        private readonly byte[] signingKey =
            RandomNumberGenerator.GetBytes(64);

        private readonly Mock<ITaskService> taskService = new();
        private readonly Mock<IUserRepository> userRepository = new();
        private readonly Mock<IPasswordHasher> passwordHasher = new();

        private readonly User user =
            new(Username, "stored-hash", DateTime.UtcNow)
            {
                Id = 42
            };

        public AuthenticationTests()
        {
            taskService.Setup(service => service.GetAll())
                .ReturnsAsync(new List<TaskDto>());

            userRepository.Setup(repository =>
                    repository.GetByUsername(Username))
                .ReturnsAsync(user);

            passwordHasher.Setup(hasher =>
                    hasher.Verify(Password, user.PasswordHash))
                .Returns(true);
        }

        private WebApplicationFactory<Program> CreateFactory()
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");

                    builder.UseContentRoot(
                        @"C:\Users\efebo\Desktop\TaskAPI\TaskAPI.API");

                    builder.UseSetting("Jwt:Issuer", Issuer);
                    builder.UseSetting("Jwt:Audience", Audience);

                    builder.UseSetting(
                        "Jwt:AccessTokenLifeTimeSeconds",
                        "1800");

                    builder.UseSetting(
                        "Jwt:SigningKey",
                        Convert.ToBase64String(signingKey));

                    builder.UseSetting(
                        "key",
                        Convert.ToBase64String(
                            RandomNumberGenerator.GetBytes(32)));

                    builder.ConfigureServices(services =>
                    {
                        services.RemoveAll<ITaskService>();
                        services.RemoveAll<IUserRepository>();
                        services.RemoveAll<IPasswordHasher>();

                        services.AddSingleton<ITaskService>(
                            taskService.Object);

                        services.AddSingleton<IUserRepository>(
                            userRepository.Object);

                        services.AddSingleton<IPasswordHasher>(
                            passwordHasher.Object);
                    });
                });
        }

        private static HttpClient CreateClient(
            WebApplicationFactory<Program> factory)
        {
            return factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    BaseAddress = new Uri("https://localhost"),
                    AllowAutoRedirect = false
                });
        }

        private string CreateToken(
            string issuer = Issuer,
            string audience = Audience,
            DateTime? expires = null,
            DateTime? notBefore = null,
            byte[]? key = null,
            string algorithm = SecurityAlgorithms.HmacSha256,
            bool includeExpiration = true)
        {
            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: new[]
                {
                    new Claim("sub", "42")
                },
                notBefore: notBefore,
                expires: includeExpiration
                    ? expires ?? DateTime.UtcNow.AddMinutes(5)
                    : null,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key ?? signingKey),
                    algorithm));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private static string ChangePayload(string token)
        {
            var parts = token.Split('.');

            // Payload değiştirilir; eski imza korunur.
            var json = Encoding.UTF8.GetString(
                Base64UrlEncoder.DecodeBytes(parts[1]));

            json = json.Replace("\"42\"", "\"999\"");

            parts[1] = Base64UrlEncoder.Encode(
                Encoding.UTF8.GetBytes(json));

            return string.Join(".", parts);
        }

        private string CreateInvalidToken(string scenario)
        {
            return scenario switch
            {
                "malformed" => "invalid-token",

                "expired" => CreateToken(
                    expires: DateTime.UtcNow.AddMinutes(-5)),

                "wrong-key" => CreateToken(
                    key: RandomNumberGenerator.GetBytes(64)),

                "wrong-issuer" => CreateToken(
                    issuer: "OtherIssuer"),

                "wrong-audience" => CreateToken(
                    audience: "OtherApi"),

                "no-expiration" => CreateToken(
                    includeExpiration: false),

                "future-not-before" => CreateToken(
                    notBefore: DateTime.UtcNow.AddMinutes(5),
                    expires: DateTime.UtcNow.AddMinutes(10)),

                "wrong-algorithm" => CreateToken(
                    algorithm: SecurityAlgorithms.HmacSha384),

                "changed-payload" => ChangePayload(CreateToken()),

                "unsigned" =>
                    new JwtSecurityTokenHandler().WriteToken(
                        new JwtSecurityToken(
                            issuer: Issuer,
                            audience: Audience,
                            claims: new[] { new Claim("sub", "42") },
                            expires: DateTime.UtcNow.AddMinutes(5))),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(scenario))
            };
        }

        [Fact]
        public async Task GetTasks_WithoutToken_Returns401()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var response = await client.GetAsync("/api/Task");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);

            taskService.Verify(
                service => service.GetAll(),
                Times.Never);
        }

        [Theory]
        [InlineData("malformed")]
        [InlineData("expired")]
        [InlineData("wrong-key")]
        [InlineData("wrong-issuer")]
        [InlineData("wrong-audience")]
        [InlineData("no-expiration")]
        [InlineData("future-not-before")]
        [InlineData("wrong-algorithm")]
        [InlineData("changed-payload")]
        [InlineData("unsigned")]
        public async Task GetTasks_InvalidToken_Returns401(
            string scenario)
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    CreateInvalidToken(scenario));

            using var response = await client.GetAsync("/api/Task");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);

            taskService.Verify(
                service => service.GetAll(),
                Times.Never);
        }

        [Fact]
        public async Task GetTasks_ValidToken_Returns200()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    CreateToken());

            using var response = await client.GetAsync("/api/Task");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks =
                await response.Content.ReadFromJsonAsync<List<TaskDto>>();

            Assert.NotNull(tasks);
            Assert.Empty(tasks);

            taskService.Verify(
                service => service.GetAll(),
                Times.Once);
        }

        [Fact]
        public async Task GetTasks_WrongAuthorizationScheme_Returns401()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Basic",
                    CreateToken());

            using var response = await client.GetAsync("/api/Task");

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);

            taskService.Verify(
                service => service.GetAll(),
                Times.Never);
        }

        [Theory]
        [InlineData("GET", "/api/Task")]
        [InlineData("GET", "/api/Task/42")]
        [InlineData("POST", "/api/Task")]
        [InlineData("PUT", "/api/Task/42")]
        [InlineData("DELETE", "/api/Task/42")]
        public async Task TaskEndpoints_WithoutToken_Return401(
            string method,
            string path)
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var request =
                new HttpRequestMessage(new HttpMethod(method), path);

            if (method is "POST" or "PUT")
            {
                request.Content = JsonContent.Create(new
                {
                    title = "Test task",
                    dueDate = DateTime.UtcNow.AddDays(1)
                });
            }

            using var response = await client.SendAsync(request);

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);

            // Hiçbir görev işlemi çağrılmamalı.
            Assert.Empty(taskService.Invocations);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUsableToken()
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            var before = DateTime.UtcNow;

            using var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/Login",
                new LoginRequestDto
                {
                    Username = Username,
                    Password = Password
                });

            var after = DateTime.UtcNow;

            Assert.Equal(
                HttpStatusCode.OK,
                loginResponse.StatusCode);

            var result = await loginResponse.Content
                .ReadFromJsonAsync<LoginResponseDto>();

            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));

            Assert.InRange(
                result.ExpireAt,
                before.AddSeconds(1800),
                after.AddSeconds(1800));

            var handler = new JwtSecurityTokenHandler
            {
                MapInboundClaims = false
            };

            var principal = handler.ValidateToken(
                result.AccessToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(signingKey),
                    ValidAlgorithms =
                        new[] { SecurityAlgorithms.HmacSha256 },

                    ValidateIssuer = true,
                    ValidIssuer = Issuer,

                    ValidateAudience = true,
                    ValidAudience = Audience,

                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero
                },
                out var validatedToken);

            Assert.Equal("42", principal.FindFirst("sub")?.Value);

            var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);

            Assert.True(
                Math.Abs((jwt.ValidTo - result.ExpireAt).TotalSeconds) < 1);

            Assert.Null(principal.FindFirst("password"));
            Assert.Null(principal.FindFirst("passwordHash"));

            // Login'in gerçekten ürettiği token ile endpoint'e eriş.
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    result.AccessToken);

            using var taskResponse = await client.GetAsync("/api/Task");

            Assert.Equal(
                HttpStatusCode.OK,
                taskResponse.StatusCode);

            passwordHasher.Verify(
                hasher => hasher.Verify(Password, user.PasswordHash),
                Times.Once);

            taskService.Verify(
                service => service.GetAll(),
                Times.Once);
        }

        [Theory]
        [InlineData("missing-user", Password)]
        [InlineData(Username, "wrong-password")]
        public async Task Login_InvalidCredentials_Returns401(
            string username,
            string password)
        {
            using var factory = CreateFactory();
            using var client = CreateClient(factory);

            using var response = await client.PostAsJsonAsync(
                "/api/Auth/Login",
                new LoginRequestDto
                {
                    Username = username,
                    Password = password
                });

            Assert.Equal(
                HttpStatusCode.Unauthorized,
                response.StatusCode);

            if (username != Username)
            {
                passwordHasher.Verify(
                    hasher => hasher.Verify(
                        It.IsAny<string>(),
                        It.IsAny<string>()),
                    Times.Never);
            }
            else
            {
                passwordHasher.Verify(
                    hasher => hasher.Verify(password, user.PasswordHash),
                    Times.Once);
            }

            Assert.Empty(taskService.Invocations);
        }
    }
}