using Microsoft.AspNetCore.Http;
using System.Text.Json;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Middleware;
using Xunit;

namespace TaskAPI.Application.Tests
{
    public class ErrorHandlerTests
    {
        [Theory]
        [InlineData(ErrorCodes.InvalidTaskFormat, 400)]
        [InlineData(ErrorCodes.TaskNotFound, 404)]
        [InlineData(ErrorCodes.GenericException, 500)]
        [InlineData(ErrorCodes.InternalServerError, 500)]
        public async Task Handler_ReturnsExpectedError(
            string errorCode,
            int statusCode)
        {
            var context = new DefaultHttpContext();

            using var body = new MemoryStream();
            context.Response.Body = body;

            var startedAt = DateTime.UtcNow;

            var handler = new ErrorHandler(_ =>
            {
                // Hata kodu otomatik GenericException seçilir.
                if (errorCode == ErrorCodes.GenericException)
                    throw new TaskApiException("Test error.", statusCode);

                // Durum kodu otomatik 500 seçilir.
                if (errorCode == ErrorCodes.InternalServerError)
                    throw new TaskApiException(errorCode, "Test error.");

                // Hata kodu ve durum kodu elle verilir.
                throw new TaskApiException(
                    errorCode,
                    "Test error.",
                    statusCode);
            });

            await handler.Invoke(context);

            var finishedAt = DateTime.UtcNow;

            body.Position = 0;

            using var json = await JsonDocument.ParseAsync(body);

            var response = json.RootElement;
            var error = response.GetProperty("error");

            // Gerçek HTTP durum kodu.
            Assert.Equal(statusCode, context.Response.StatusCode);

            // Cevabın genel yapısı.
            Assert.False(
                response.GetProperty("success").GetBoolean());

            Assert.Equal(
                JsonValueKind.Null,
                response.GetProperty("result").ValueKind);

            // Hata bilgileri.
            Assert.Equal(
                errorCode,
                error.GetProperty("errorCode").GetString());

            Assert.Equal(
                statusCode,
                error.GetProperty("statusCode").GetInt32());

            Assert.Equal(
                "Test error.",
                error.GetProperty("message").GetString());

            // Takip numarası geçerli bir GUID olmalı.
            var correlationId =
                error.GetProperty("correlationId").GetString();

            Assert.True(Guid.TryParse(correlationId, out _));

            // Hata zamanı test sırasında oluşturulmuş olmalı.
            var timeStamp =
                error.GetProperty("timeStamp").GetDateTime();

            Assert.InRange(timeStamp, startedAt, finishedAt);
        }
    }
}