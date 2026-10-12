using System.Runtime.CompilerServices;
using System.Text.Json;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Middleware;
using TaskAPI.Core.Models;
using Xunit;

namespace TaskAPI.API.Tests
{
    public class ErrorModelTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Constructors_CreateExpectedErrorInformation(int constructor)
        {
            var before = DateTime.UtcNow;
            var suppliedId = Guid.NewGuid().ToString();

            var model = constructor switch
            {
                0 => new ErrorModel(ErrorCodes.InternalServerError, "Test message."),
                1 => new ErrorModel(ErrorCodes.BadRequest, "Test message.", 400),
                2 => new ErrorModel(ErrorCodes.Conflict, "Test detail.", "Test message.", 409),
                3 => new ErrorModel(ErrorCodes.TaskNotFound, "Test detail.", "Test message.", suppliedId, 404),
                _ => throw new ArgumentOutOfRangeException(nameof(constructor))
            };

            var expectedCode = constructor switch
            {
                0 => ErrorCodes.InternalServerError,
                1 => ErrorCodes.BadRequest,
                2 => ErrorCodes.Conflict,
                _ => ErrorCodes.TaskNotFound
            };

            var expectedStatus = constructor switch
            {
                0 => 500,
                1 => 400,
                2 => 409,
                _ => 404
            };

            Assert.Equal(expectedCode, model.ErrorCode);
            Assert.Equal(expectedStatus, model.StatusCode);
            Assert.Equal("Test message.", model.Message);
            Assert.Equal(constructor >= 2 ? "Test detail." : null, model.Description);
            Assert.True(Guid.TryParse(model.CorrelationId, out _));
            Assert.InRange(model.TimeStamp, before, DateTime.UtcNow);

            if (constructor == 3)
                Assert.Equal(suppliedId, model.CorrelationId);
        }

        [Fact]
        public void SeparateErrors_HaveDifferentCorrelationIds()
        {
            var first = new ErrorModel(ErrorCodes.BadRequest, "First.", 400);
            var second = new ErrorModel(ErrorCodes.BadRequest, "Second.", 400);

            Assert.NotEqual(first.CorrelationId, second.CorrelationId);
        }
    }

    public class TaskApiExceptionTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Constructors_PreserveMessageCodeStatusAndInnerError(int constructor)
        {
            var original = new InvalidOperationException("Original error.");
            var before = DateTime.UtcNow;

            var exception = constructor switch
            {
                0 => new TaskApiException("Test message.", 400),
                1 => new TaskApiException(ErrorCodes.InvalidTaskFormat, "Test message."),
                2 => new TaskApiException(ErrorCodes.TaskNotFound, "Test message.", 404),
                3 => new TaskApiException(ErrorCodes.InternalServerError, "Test message.", 500, original),
                _ => throw new ArgumentOutOfRangeException(nameof(constructor))
            };

            var expectedCode = constructor switch
            {
                0 => ErrorCodes.GenericException,
                1 => ErrorCodes.InvalidTaskFormat,
                2 => ErrorCodes.TaskNotFound,
                _ => ErrorCodes.InternalServerError
            };

            var expectedStatus = constructor switch
            {
                0 => 400,
                2 => 404,
                _ => 500
            };

            Assert.Equal(expectedCode, exception.errorModel.ErrorCode);
            Assert.Equal(expectedStatus, exception.errorModel.StatusCode);
            Assert.Equal("Test message.", exception.Message);
            Assert.Equal("Test message.", exception.errorModel.Message);
            Assert.True(Guid.TryParse(exception.errorModel.CorrelationId, out _));
            Assert.InRange(exception.errorModel.TimeStamp, before, DateTime.UtcNow);

            if (constructor == 3)
                Assert.Same(original, exception.InnerException);
            else
                Assert.Null(exception.InnerException);
        }
    }

    public class ErrorHandlerTests
    {
        [Theory]
        [InlineData(ErrorCodes.InvalidTaskFormat, 500)]
        [InlineData(ErrorCodes.BadRequest, 400)]
        [InlineData(ErrorCodes.TaskNotFound, 404)]
        [InlineData(ErrorCodes.Unauthorized, 401)]
        [InlineData(ErrorCodes.Conflict, 409)]
        [InlineData(ErrorCodes.InternalServerError, 500)]
        public async Task CustomError_PreservesItsEntireModel(string code, int status)
        {
            var exception = new TaskApiException(code, "Test message.", status);
            exception.errorModel.Description = "Test detail.";

            var context = new DefaultHttpContext();
            using var body = new MemoryStream();
            context.Response.Body = body;

            var handler = new ErrorHandler(_ => throw exception);

            await handler.Invoke(context);

            body.Position = 0;
            using var json = await JsonDocument.ParseAsync(body);

            var response = json.RootElement;
            var error = response.GetProperty("error");

            Assert.Equal(status, context.Response.StatusCode);
            Assert.StartsWith("application/json", context.Response.ContentType!);
            Assert.False(response.GetProperty("success").GetBoolean());
            Assert.Equal(JsonValueKind.Null, response.GetProperty("result").ValueKind);

            Assert.Equal(code, error.GetProperty("errorCode").GetString());
            Assert.Equal(status, error.GetProperty("statusCode").GetInt32());
            Assert.Equal("Test message.", error.GetProperty("message").GetString());
            Assert.Equal("Test detail.", error.GetProperty("description").GetString());
            Assert.Equal(exception.errorModel.CorrelationId, error.GetProperty("correlationId").GetString());
            Assert.Equal(exception.errorModel.TimeStamp, error.GetProperty("timeStamp").GetDateTime());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task StandardErrors_AreConvertedToExpectedResponses(bool argumentError)
        {
            Exception exception = argumentError
                ? new ArgumentException("Invalid argument.")
                : new InvalidOperationException("Unexpected error.");

            var context = new DefaultHttpContext();
            using var body = new MemoryStream();
            context.Response.Body = body;

            await new ErrorHandler(_ => throw exception).Invoke(context);

            body.Position = 0;
            using var json = await JsonDocument.ParseAsync(body);
            var error = json.RootElement.GetProperty("error");

            var expectedStatus = argumentError ? 400 : 500;
            var expectedCode = argumentError ? ErrorCodes.BadRequest : ErrorCodes.InternalServerError;

            Assert.Equal(expectedStatus, context.Response.StatusCode);
            Assert.Equal(expectedStatus, error.GetProperty("statusCode").GetInt32());
            Assert.Equal(expectedCode, error.GetProperty("errorCode").GetString());
            Assert.Equal(exception.Message, error.GetProperty("message").GetString());
            Assert.True(Guid.TryParse(error.GetProperty("correlationId").GetString(), out _));
            Assert.NotEqual(default, error.GetProperty("timeStamp").GetDateTime());
        }

        [Fact]
        public async Task SuccessfulRequest_IsNotReplacedWithAnError()
        {
            var context = new DefaultHttpContext();
            using var body = new MemoryStream();
            context.Response.Body = body;

            var handler = new ErrorHandler(async current =>
            {
                current.Response.StatusCode = 201;
                await current.Response.WriteAsync("Successful response.");
            });

            await handler.Invoke(context);

            body.Position = 0;
            using var reader = new StreamReader(body);

            Assert.Equal(201, context.Response.StatusCode);
            Assert.Equal("Successful response.", await reader.ReadToEndAsync());
        }
    }

    public class MethodExecutorTests
    {
        private static async Task InvokeExecutor(string method, Func<Task> asyncOperation, Action syncOperation, ILogger logger, IHttpContextAccessor accessor)
        {
            switch (method)
            {
                case "ExecuteAsync":
                    await MethodExecutor.ExecuteAsync(async () =>
                    {
                        await asyncOperation();
                        return 7;
                    }, logger, accessor, "Test operation");
                    break;

                case "Execute":
                    MethodExecutor.Execute(() =>
                    {
                        syncOperation();
                        return 7;
                    }, logger, accessor, "Test operation");
                    break;

                case "RunAsync":
                    await MethodExecutor.RunAsync(asyncOperation, logger, accessor, "Test operation");
                    break;

                case "Run":
                    MethodExecutor.Run(syncOperation, logger, accessor, "Test operation");
                    break;

                default:
                    throw new ArgumentException("Unknown executor method.");
            }
        }

        [Theory]
        [InlineData("ExecuteAsync")]
        [InlineData("Execute")]
        [InlineData("RunAsync")]
        [InlineData("Run")]
        public async Task SuccessfulOperation_RunsOnceAndLogsStartAndCompletion(string method)
        {
            var logger = new CollectingLogger();
            var calls = 0;

            await InvokeExecutor(
                method,
                () =>
                {
                    calls++;
                    return Task.CompletedTask;
                },
                () => calls++,
                logger,
                TestSupport.CreateAccessor());

            Assert.Equal(1, calls);
            Assert.Equal(2, logger.Entries.Count);
            Assert.All(logger.Entries, entry => Assert.Equal(LogLevel.Information, entry.Level));
            Assert.Contains("started", logger.Entries[0].Message);
            Assert.Contains("completed successfully", logger.Entries[1].Message);
            Assert.Contains("/api/Task/7", logger.Entries[0].Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ExecuteMethods_ReturnTheOperationResult(bool asynchronous)
        {
            var logger = new CollectingLogger();
            var accessor = TestSupport.CreateAccessor();

            var result = asynchronous
                ? await MethodExecutor.ExecuteAsync(() => Task.FromResult(42), logger, accessor, "Test")
                : MethodExecutor.Execute(() => 42, logger, accessor, "Test");

            Assert.Equal(42, result);
        }

        [Theory]
        [InlineData("ExecuteAsync")]
        [InlineData("Execute")]
        [InlineData("RunAsync")]
        [InlineData("Run")]
        public async Task ArgumentError_IsLoggedAndRethrownWithoutReplacement(string method)
        {
            var original = new ArgumentException("Invalid argument.");
            var logger = new CollectingLogger();

            var actual = await Assert.ThrowsAsync<ArgumentException>(() => InvokeExecutor(method, () => throw original, () => throw original, logger, TestSupport.CreateAccessor()));

            Assert.Same(original, actual);
            Assert.Contains(logger.Entries, entry => entry.Level == LogLevel.Warning);
            Assert.DoesNotContain(logger.Entries, entry => entry.Message.Contains("completed successfully"));
        }

        [Theory]
        [InlineData("ExecuteAsync")]
        [InlineData("Execute")]
        [InlineData("RunAsync")]
        [InlineData("Run")]
        public async Task CustomError_IsNotWrappedAgain(string method)
        {
            var original = new TaskApiException(ErrorCodes.TaskNotFound, "Missing task.", 404);
            var logger = new CollectingLogger();

            var actual = await Assert.ThrowsAsync<TaskApiException>(() => InvokeExecutor(method, () => throw original, () => throw original, logger, TestSupport.CreateAccessor()));

            Assert.Same(original, actual);
            Assert.Equal(404, actual.errorModel.StatusCode);
            Assert.Contains(logger.Entries, entry => ReferenceEquals(entry.Exception, original));
        }

        [Theory]
        [InlineData("ExecuteAsync")]
        [InlineData("Execute")]
        [InlineData("RunAsync")]
        [InlineData("Run")]
        public async Task UnexpectedError_UsesEachMethodsCurrentBehavior(string method)
        {
            var original = new InvalidOperationException("Original error.");
            var logger = new CollectingLogger();

            if (method == "ExecuteAsync")
            {
                var actual = await Assert.ThrowsAsync<TaskApiException>(() => InvokeExecutor(method, () => throw original, () => throw original, logger, TestSupport.CreateAccessor()));

                Assert.Same(original, actual.InnerException);
                Assert.Equal(ErrorCodes.InternalServerError, actual.errorModel.ErrorCode);
                Assert.Equal(500, actual.errorModel.StatusCode);
                Assert.Equal("Test operation could not be completed.", actual.Message);
            }
            else
            {
                var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeExecutor(method, () => throw original, () => throw original, logger, TestSupport.CreateAccessor()));

                Assert.Same(original, actual);
            }
        }

        [Theory]
        [InlineData("ExecuteAsync")]
        [InlineData("Execute")]
        [InlineData("RunAsync")]
        [InlineData("Run")]
        public async Task MissingHttpContext_PreventsOperationFromRunning(string method)
        {
            var called = false;
            var logger = new CollectingLogger();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeExecutor(
                method,
                () =>
                {
                    called = true;
                    return Task.CompletedTask;
                },
                () => called = true,
                logger,
                new HttpContextAccessor()));

            Assert.Equal("No active HTTP request found", exception.Message);
            Assert.False(called);
            Assert.Empty(logger.Entries);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(10)]
        [InlineData(50)]
        public async Task ExecuteAsync_LogsEveryInnerExceptionLayer(int layers)
        {
            Exception? inner = null;

            for (var depth = layers; depth >= 1; depth--)
                inner = new InvalidOperationException($"Layer {depth}", inner);

            var original = inner == null
                ? new TaskApiException(ErrorCodes.InternalServerError, "Outer error.", 500)
                : new TaskApiException(ErrorCodes.InternalServerError, "Outer error.", 500, inner);

            var logger = new CollectingLogger();

            var actual = await Assert.ThrowsAsync<TaskApiException>(() => MethodExecutor.ExecuteAsync<int>(() => throw original, logger, TestSupport.CreateAccessor(), "Test"));

            Assert.Same(original, actual);

            var layerLogs = logger.Entries.Where(entry => entry.Message.Contains("inner error at depth")).ToList();

            Assert.Equal(layers, layerLogs.Count);

            for (var index = 0; index < layerLogs.Count; index++)
            {
                Assert.Contains($"depth {index + 1}:", layerLogs[index].Message);
                Assert.Contains($"Layer {index + 1}", layerLogs[index].Message);
            }
        }

        [Fact]
        public async Task NestedExecutors_WrapUnexpectedErrorOnlyOnce()
        {
            var original = new InvalidOperationException("Repository error.");
            var logger = new CollectingLogger();
            var accessor = TestSupport.CreateAccessor();

            var actual = await Assert.ThrowsAsync<TaskApiException>(() => MethodExecutor.ExecuteAsync(
                () => MethodExecutor.ExecuteAsync<int>(() => throw original, logger, accessor, "Service"),
                logger,
                accessor,
                "Controller"));

            Assert.Same(original, actual.InnerException);
            Assert.Equal("Service could not be completed.", actual.Message);
            Assert.IsNotType<TaskApiException>(actual.InnerException);
        }

        [Fact]
        public async Task Wrapping_PreservesOriginalCallPath()
        {
            var logger = new CollectingLogger();

            var exception = await Assert.ThrowsAsync<TaskApiException>(() => MethodExecutor.ExecuteAsync(() => Task.FromResult(CallService()), logger, TestSupport.CreateAccessor(), "Get"));

            var original = Assert.IsType<InvalidOperationException>(exception.InnerException);

            Assert.Contains(nameof(CallService), original.StackTrace!);
            Assert.Contains(nameof(CallRepository), original.StackTrace!);
            Assert.Contains(nameof(FailAtHelper), original.StackTrace!);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CallService()
        {
            return CallRepository();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CallRepository()
        {
            return FailAtHelper();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int FailAtHelper()
        {
            throw new InvalidOperationException("Helper error.");
        }
        [Fact]
        public async Task InvalidLogin_Returns401Response()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            var middleware = new ErrorHandler(_ =>
                throw new TaskApiException(
                    ErrorCodes.Unauthorized,
                    "Invalid username or password.",
                    401));

            await middleware.Invoke(context);

            Assert.Equal(401, context.Response.StatusCode);
        }
    }
}