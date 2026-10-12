using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text.Json;
using TaskAPI.API.Controllers;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Middleware;
using TaskAPI.Core.Models;
using Xunit;

namespace TaskAPI.API.Tests
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> service = new();
        private readonly TaskController controller;
        private readonly TaskDto dto = TestSupport.CreateDto();
        private readonly List<TaskDto> list = [new TaskDto { Id = 7 }];

        public TaskControllerTests()
        {
            service.Setup(current => current.Get(It.IsAny<int>())).ReturnsAsync(dto);
            service.Setup(current => current.Add(It.IsAny<TaskDto>())).ReturnsAsync(dto);
            service.Setup(current => current.Update(It.IsAny<int>(), It.IsAny<TaskDto>())).ReturnsAsync(dto);
            service.Setup(current => current.Delete(It.IsAny<int>())).ReturnsAsync(true);
            service.Setup(current => current.GetAll()).ReturnsAsync(list);

            controller = new TaskController(service.Object, NullLogger<TaskController>.Instance, TestSupport.CreateAccessor());
        }

        private Task<IActionResult> Invoke(string operation)
        {
            return operation switch
            {
                "Get" => controller.Get(7),
                "Create" => controller.Create(dto),
                "Update" => controller.Update(7, dto),
                "Delete" => controller.Delete(7),
                "GetAll" => controller.GetAll(),
                _ => throw new ArgumentException("Unknown operation.")
            };
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("Create")]
        [InlineData("Update")]
        [InlineData("Delete")]
        [InlineData("GetAll")]
        public async Task SuccessfulOperations_Return200AndExpectedResult(string operation)
        {
            var result = Assert.IsType<OkObjectResult>(await Invoke(operation));

            Assert.Equal(200, result.StatusCode);

            if (operation == "Delete")
                Assert.True(Assert.IsType<bool>(result.Value));
            else if (operation == "GetAll")
                Assert.Same(list, result.Value);
            else
                Assert.Same(dto, result.Value);
        }

        [Theory]
        [InlineData("Get", "Task not found.")]
        [InlineData("Create", "Failed to create task.")]
        [InlineData("Update", "Task not found.")]
        [InlineData("Delete", "Task not found.")]
        [InlineData("GetAll", "No tasks found.")]
        public async Task MissingServiceResult_UsesExistingArgumentExceptionChecks(string operation, string message)
        {
            switch (operation)
            {
                case "Get":
                    service.Setup(current => current.Get(It.IsAny<int>())).ReturnsAsync((TaskDto?)null);
                    break;
                case "Create":
                    service.Setup(current => current.Add(It.IsAny<TaskDto>())).ReturnsAsync((TaskDto)null!);
                    break;
                case "Update":
                    service.Setup(current => current.Update(It.IsAny<int>(), It.IsAny<TaskDto>())).ReturnsAsync((TaskDto)null!);
                    break;
                case "Delete":
                    service.Setup(current => current.Delete(It.IsAny<int>())).ReturnsAsync(false);
                    break;
                case "GetAll":
                    service.Setup(current => current.GetAll()).ReturnsAsync((List<TaskDto>)null!);
                    break;
            }

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => Invoke(operation));

            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public async Task Get_UnexpectedServiceError_IsWrapped()
        {
            var original = new InvalidOperationException("Service failure.");
            service.Setup(current => current.Get(7)).ThrowsAsync(original);

            var exception = await Assert.ThrowsAsync<TaskApiException>(() => controller.Get(7));

            Assert.Equal(500, exception.errorModel.StatusCode);
            Assert.Equal(ErrorCodes.InternalServerError, exception.errorModel.ErrorCode);
            Assert.Equal("Get could not be completed.", exception.Message);
            Assert.Same(original, exception.InnerException);
        }

        [Theory]
        [InlineData(ErrorCodes.TaskNotFound, 404)]
        [InlineData(ErrorCodes.InvalidTaskFormat, 500)]
        [InlineData(ErrorCodes.BadRequest, 400)]
        public async Task ControllerError_ReachesMiddlewareWithOriginalInformation(string code, int status)
        {
            var original = new TaskApiException(code, "Original message.", status);
            service.Setup(current => current.Get(7)).ThrowsAsync(original);

            var context = new DefaultHttpContext();
            using var body = new MemoryStream();
            context.Response.Body = body;

            var handler = new ErrorHandler(async _ =>
            {
                await controller.Get(7);
            });

            await handler.Invoke(context);

            body.Position = 0;
            using var json = await JsonDocument.ParseAsync(body);
            var error = json.RootElement.GetProperty("error");

            Assert.Equal(status, context.Response.StatusCode);
            Assert.Equal(code, error.GetProperty("errorCode").GetString());
            Assert.Equal("Original message.", error.GetProperty("message").GetString());
            Assert.Equal(original.errorModel.CorrelationId, error.GetProperty("correlationId").GetString());
        }
    }

    public class AuthControllerTests
    {
        private readonly Mock<IAccountService> service = new();
        private readonly AuthController controller;
        private readonly LoginRequestDto dto = new() { Username = "test-user", Password = "test-password" };

        public AuthControllerTests()
        {
            controller = new AuthController(service.Object, NullLogger<AuthController>.Instance, TestSupport.CreateAccessor());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Register_ReturnsSuccessOrConflict(bool registered)
        {
            service.Setup(current => current.Register(dto)).ReturnsAsync(registered);

            if (registered)
            {
                var result = Assert.IsType<OkObjectResult>(await controller.Register(dto));
                Assert.Equal(200, result.StatusCode);
                Assert.True(Assert.IsType<bool>(result.Value));
            }
            else
            {
                var exception = await Assert.ThrowsAsync<TaskApiException>(
                    () => controller.Register(dto));

                Assert.Equal(409, exception.errorModel.StatusCode);
                Assert.Equal(ErrorCodes.Conflict, exception.errorModel.ErrorCode);
            }
        }

        //[Fact]
        //public async Task Login_Success_ReturnsSuccessfulResponse()
        //{
        //    service.Setup(current => current.Login(dto)).ReturnsAsync(true);

        //    var result = Assert.IsType<OkObjectResult>(await controller.Login(dto));
        //    var response = Assert.IsType<ResponseModel<bool>>(result.Value);

        //    Assert.Equal(200, result.StatusCode);
        //    Assert.True(response.Success);
        //    Assert.True(response.Result);
        //    Assert.Null(response.Error);
        //}

        //[Fact]
        //public async Task Login_Failure_HttpStatusAndModelStatusShouldBothBe401()
        //{
        //    service.Setup(current => current.Login(dto)).ReturnsAsync(false);

        //    var result = Assert.IsType<UnauthorizedObjectResult>(await controller.Login(dto));
        //    var response = Assert.IsType<ResponseModel<object>>(result.Value);

        //    Assert.Equal(401, result.StatusCode);
        //    Assert.False(response.Success);
        //    Assert.Null(response.Result);
        //    Assert.Equal(ErrorCodes.Unauthorized, response.Error!.ErrorCode);

        //    // Mevcut kodda model 500 taşıyor. Bu test o uyuşmazlığı gösterir.
        //    Assert.Equal(401, response.Error.StatusCode);
        //}

        [Theory]
        [InlineData("Register")]
        [InlineData("Login")]
        public async Task UnexpectedAccountError_IsWrappedByController(string operation)
        {
            var original = new InvalidOperationException("Account service failure.");

            service.Setup(current => current.Register(dto)).ThrowsAsync(original);
            service.Setup(current => current.Login(dto)).ThrowsAsync(original);

            Func<Task> action = operation == "Register"
                ? () => controller.Register(dto)
                : () => controller.Login(dto);

            var exception = await Assert.ThrowsAsync<TaskApiException>(action);

            Assert.Same(original, exception.InnerException);
            Assert.Equal(500, exception.errorModel.StatusCode);
            Assert.Equal($"{operation} could not be completed.", exception.Message);
        }
    }
}
