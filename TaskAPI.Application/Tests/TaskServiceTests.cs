using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Application.Services;
using TaskAPI.Core.Helpers;
using Xunit;

namespace TaskAPI.Application.Tests
{
    public class TaskServiceTests
    {
        [Fact]
        public async Task addTaskWithEmptyTitleTest()
        {
            var repository = new Mock<ITaskRepository>();
            var mapper = new Mock<IMapper>();

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            var service = new TaskService(
                repository.Object,
                mapper.Object,
                NullLogger<TaskService>.Instance,
                httpContextAccessor);

            var dto = new TaskDto
            {
                Title = "",
                Description = "Test task",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var exception = await Assert.ThrowsAsync<TaskApiException>(
                () => service.Add(dto));

            Assert.Equal(
                ErrorCodes.InvalidTaskFormat,
                exception.errorModel.ErrorCode);

            Assert.Equal(400, exception.errorModel.StatusCode);
            Assert.Equal("Title cannot be empty.", exception.Message);

            repository.Verify(
                repo => repo.Add(It.IsAny<TaskAPI.Domain.Entity.Task>()),
                Times.Never);
        }

        [Fact]
        public async Task AddTaskWithPastDueDateTest()
        {
            var repository = new Mock<ITaskRepository>();
            var mapper = new Mock<IMapper>();

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            var service = new TaskService(
                repository.Object,
                mapper.Object,
                NullLogger<TaskService>.Instance,
                httpContextAccessor);

            var dto = new TaskDto
            {
                Title = "Test task",
                Description = "Test task",
                DueDate = DateTime.UtcNow.AddDays(-1)
            };

            var exception = await Assert.ThrowsAsync<TaskApiException>(
                () => service.Add(dto));

            Assert.Equal(
                ErrorCodes.InvalidTaskFormat,
                exception.errorModel.ErrorCode);

            Assert.Equal(400, exception.errorModel.StatusCode);
            Assert.Equal(
                "Due date cannot be in the past.",
                exception.Message);

            repository.Verify(
                repo => repo.Add(It.IsAny<TaskAPI.Domain.Entity.Task>()),
                Times.Never);
        }

        [Fact]
        public async Task getTaskWithInvalidIdTest()
        {
            var repository = new Mock<ITaskRepository>();
            var mapper = new Mock<IMapper>();

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            var service = new TaskService(
                repository.Object,
                mapper.Object,
                NullLogger<TaskService>.Instance,
                httpContextAccessor);

            int invalidId = -1;

            var exception = await Assert.ThrowsAsync<TaskApiException>(
                () => service.Get(invalidId));

            Assert.Equal(
                ErrorCodes.TaskNotFound,
                exception.errorModel.ErrorCode);

            Assert.Equal(404, exception.errorModel.StatusCode);
            Assert.Equal("Task not found.", exception.Message);

            mapper.Verify(
                map => map.Map<TaskDto>(It.IsAny<object>()),
                Times.Never);
        }
    }
}