using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Mapping;
using TaskAPI.Core.Helpers;
using Xunit;
using EntityTask = TaskAPI.Domain.Entity.Task;

namespace TaskAPI.API.Tests
{
    public static class TestSupport
    {
        public static IHttpContextAccessor CreateAccessor()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/api/Task/7";

            return new HttpContextAccessor
            {
                HttpContext = context
            };
        }

        public static TaskDto CreateDto()
        {
            return new TaskDto
            {
                Title = "Test task",
                Description = "Test description",
                DueDate = DateTime.UtcNow.AddDays(10)
            };
        }

        public static EntityTask CreateEntity()
        {
            return new EntityTask("Test task", "Test description", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(10))
            {
                Id = 7
            };
        }

        public static IMapper CreateMapper()
        {
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
            configuration.AssertConfigurationIsValid();

            return configuration.CreateMapper();
        }

        public static async Task<TaskApiException> AssertTaskError(Func<Task> operation, string code, int status, string message)
        {
            var exception = await Assert.ThrowsAsync<TaskApiException>(operation);

            Assert.Equal(code, exception.errorModel.ErrorCode);
            Assert.Equal(status, exception.errorModel.StatusCode);
            Assert.Equal(message, exception.Message);
            Assert.Equal(message, exception.errorModel.Message);
            Assert.True(Guid.TryParse(exception.errorModel.CorrelationId, out _));
            Assert.NotEqual(default, exception.errorModel.TimeStamp);

            return exception;
        }
    }

    public sealed class CapturedLog
    {
        public LogLevel Level { get; init; }
        public string Message { get; init; } = "";
        public Exception? Exception { get; init; }
    }

    public sealed class CollectingLogger : ILogger
    {
        public List<CapturedLog> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new CapturedLog
            {
                Level = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            });
        }
    }
}