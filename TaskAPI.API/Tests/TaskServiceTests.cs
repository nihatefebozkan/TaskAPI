using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Application.Services;
using TaskAPI.Core.Helpers;
using Xunit;
using EntityTask = TaskAPI.Domain.Entity.Task;

namespace TaskAPI.API.Tests
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> repository = new();
        private readonly Mock<IMapper> mapper = new();
        private readonly TaskService service;
        private readonly EntityTask entity = TestSupport.CreateEntity();
        private readonly TaskDto dto = TestSupport.CreateDto();
        private readonly TaskDto output = new() { Id = 7, Title = "Mapped task" };
        private readonly List<TaskDto> outputList = [new TaskDto { Id = 7 }];

        public TaskServiceTests()
        {
            repository.Setup(repo => repo.Get(It.IsAny<int>())).ReturnsAsync(entity);
            repository.Setup(repo => repo.GetAll()).ReturnsAsync(new List<EntityTask> { entity });
            repository.Setup(repo => repo.Add(It.IsAny<EntityTask>())).ReturnsAsync(entity);
            repository.Setup(repo => repo.Update(It.IsAny<EntityTask>())).ReturnsAsync(entity);
            repository.Setup(repo => repo.Delete(It.IsAny<EntityTask>())).ReturnsAsync(entity);

            mapper.Setup(map => map.Map<EntityTask>(It.IsAny<object>())).Returns(entity);
            mapper.Setup(map => map.Map<TaskDto>(It.IsAny<object>())).Returns(output);
            mapper.Setup(map => map.Map<List<TaskDto>>(It.IsAny<object>())).Returns(outputList);

            service = new TaskService(repository.Object, mapper.Object, NullLogger<TaskService>.Instance, TestSupport.CreateAccessor());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Add_InvalidTitle_ReturnsYourChosen500(string? title)
        {
            dto.Title = title;

            await TestSupport.AssertTaskError(() => service.Add(dto), ErrorCodes.InvalidTaskFormat, 500, "Title cannot be empty.");

            repository.Verify(repo => repo.Add(It.IsAny<EntityTask>()), Times.Never);
            mapper.Verify(map => map.Map<EntityTask>(It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task Add_PastDate_ReturnsYourChosen500()
        {
            dto.DueDate = DateTime.UtcNow.AddDays(-1);

            await TestSupport.AssertTaskError(() => service.Add(dto), ErrorCodes.InvalidTaskFormat, 500, "Due date cannot be in the past.");

            repository.Verify(repo => repo.Add(It.IsAny<EntityTask>()), Times.Never);
        }

        [Fact]
        public async Task Add_ValidInput_MapsSavesAndReturnsDto()
        {
            var result = await service.Add(dto);

            Assert.Same(output, result);
            mapper.Verify(map => map.Map<EntityTask>(dto), Times.Once);
            repository.Verify(repo => repo.Add(entity), Times.Once);
            mapper.Verify(map => map.Map<TaskDto>(entity), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Add_NullMappingResult_ReportsCorrectStage(bool inputMapping)
        {
            if (inputMapping)
                mapper.Setup(map => map.Map<EntityTask>(It.IsAny<object>())).Returns((EntityTask)null!);
            else
                mapper.Setup(map => map.Map<TaskDto>(It.IsAny<object>())).Returns((TaskDto)null!);

            var expectedMessage = inputMapping
                ? "Task could not be mapped to domain entity"
                : "Task could not be mapped to DTO";

            await TestSupport.AssertTaskError(() => service.Add(dto), ErrorCodes.InternalServerError, 500, expectedMessage);

            repository.Verify(repo => repo.Add(It.IsAny<EntityTask>()), inputMapping ? Times.Never() : Times.Once());
        }

        [Fact]
        public async Task Get_ExistingTask_ReturnsMappedDto()
        {
            var result = await service.Get(7);

            Assert.Same(output, result);
            repository.Verify(repo => repo.Get(7), Times.Once);
            mapper.Verify(map => map.Map<TaskDto>(entity), Times.Once);
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("Update")]
        [InlineData("Delete")]
        public async Task MissingTask_Returns404AndDoesNotModifyAnything(string operation)
        {
            repository.Setup(repo => repo.Get(999)).ReturnsAsync((EntityTask)null!);

            Func<Task> action = operation switch
            {
                "Get" => () => service.Get(999),
                "Update" => () => service.Update(999, dto),
                "Delete" => () => service.Delete(999),
                _ => throw new ArgumentException("Unknown operation.")
            };

            await TestSupport.AssertTaskError(action, ErrorCodes.TaskNotFound, 404, "Task not found.");

            repository.Verify(repo => repo.Update(It.IsAny<EntityTask>()), Times.Never);
            repository.Verify(repo => repo.Delete(It.IsAny<EntityTask>()), Times.Never);
            mapper.Verify(map => map.Map<TaskDto>(It.IsAny<object>()), Times.Never);
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("Update")]
        public async Task NullOutputMapping_ReturnsInternalError(string operation)
        {
            mapper.Setup(map => map.Map<TaskDto>(It.IsAny<object>())).Returns((TaskDto)null!);

            Func<Task> action = operation == "Get"
                ? () => service.Get(7)
                : () => service.Update(7, dto);

            await TestSupport.AssertTaskError(action, ErrorCodes.InternalServerError, 500, "Task could not be mapped");
        }

        [Fact]
        public async Task GetAll_ReturnsMappedList()
        {
            var result = await service.GetAll();

            Assert.Same(outputList, result);
            repository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetAll_EmptyOrNullRepositoryResult_IsMappedAsEmptyList(bool nullResult)
        {
            repository.Setup(repo => repo.GetAll()).ReturnsAsync(nullResult ? null! : new List<EntityTask>());

            mapper.Setup(map => map.Map<List<TaskDto>>(It.IsAny<object>())).Returns((object source) =>
            {
                Assert.Empty(Assert.IsType<List<EntityTask>>(source));
                return [];
            });

            var result = await service.GetAll();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAll_NullMappingResult_ReturnsInternalError()
        {
            mapper.Setup(map => map.Map<List<TaskDto>>(It.IsAny<object>())).Returns((List<TaskDto>)null!);

            await TestSupport.AssertTaskError(() => service.GetAll(), ErrorCodes.InternalServerError, 500, "Tasks could not be mapped");
        }

        [Fact]
        public async Task Update_ChangesOnlyDueDate()
        {
            var oldTitle = entity.Title;
            var oldDescription = entity.Description;
            var oldCreatedAt = entity.CreatedAt;

            entity.IsCompleted = true;
            dto.Title = "Ignored new title";
            dto.Description = "Ignored new description";
            dto.CreatedAt = DateTime.UtcNow.AddYears(-10);
            dto.IsCompleted = false;

            var result = await service.Update(7, dto);

            Assert.Same(output, result);
            Assert.Equal(dto.DueDate, entity.DueDate);
            Assert.Equal(oldTitle, entity.Title);
            Assert.Equal(oldDescription, entity.Description);
            Assert.Equal(oldCreatedAt, entity.CreatedAt);
            Assert.True(entity.IsCompleted);
            Assert.Null(entity.DeletedAt);

            repository.Verify(repo => repo.Update(entity), Times.Once);
        }

        [Fact]
        public async Task Update_PastDate_DoesNotSave()
        {
            dto.DueDate = DateTime.UtcNow.AddDays(-1);
            var oldDate = entity.DueDate;

            await TestSupport.AssertTaskError(() => service.Update(7, dto), ErrorCodes.InvalidTaskFormat, 500, "Due date cannot be in the past.");

            Assert.Equal(oldDate, entity.DueDate);
            repository.Verify(repo => repo.Update(It.IsAny<EntityTask>()), Times.Never);
        }

        [Fact]
        public async Task Delete_MarksDeletionAndCallsRepository()
        {
            var before = DateTimeOffset.UtcNow;

            var result = await service.Delete(7);

            Assert.True(result);
            Assert.NotNull(entity.DeletedAt);
            Assert.InRange(entity.DeletedAt.Value, before, DateTimeOffset.UtcNow);
            repository.Verify(repo => repo.Delete(entity), Times.Once);
        }

        [Theory]
        [InlineData("Add")]
        [InlineData("Get")]
        [InlineData("GetAll")]
        [InlineData("Update")]
        [InlineData("Delete")]
        public async Task RepositoryFailure_IsWrappedAndOriginalErrorIsPreserved(string operation)
        {
            var original = new InvalidOperationException("Repository failure.");

            switch (operation)
            {
                case "Add":
                    repository.Setup(repo => repo.Add(It.IsAny<EntityTask>())).ThrowsAsync(original);
                    break;
                case "Get":
                    repository.Setup(repo => repo.Get(It.IsAny<int>())).ThrowsAsync(original);
                    break;
                case "GetAll":
                    repository.Setup(repo => repo.GetAll()).ThrowsAsync(original);
                    break;
                case "Update":
                    repository.Setup(repo => repo.Update(It.IsAny<EntityTask>())).ThrowsAsync(original);
                    break;
                case "Delete":
                    repository.Setup(repo => repo.Delete(It.IsAny<EntityTask>())).ThrowsAsync(original);
                    break;
            }

            Func<Task> action = operation switch
            {
                "Add" => () => service.Add(dto),
                "Get" => () => service.Get(7),
                "GetAll" => () => service.GetAll(),
                "Update" => () => service.Update(7, dto),
                "Delete" => () => service.Delete(7),
                _ => throw new ArgumentException("Unknown operation.")
            };

            var exception = await TestSupport.AssertTaskError(action, ErrorCodes.InternalServerError, 500, $"{operation} could not be completed.");

            Assert.Same(original, exception.InnerException);
        }

        [Fact]
        public async Task RepositoryCustomError_IsPreservedWithoutAnotherWrapper()
        {
            var original = new TaskApiException(ErrorCodes.BadRequest, "Repository custom error.", 400);
            repository.Setup(repo => repo.Get(7)).ThrowsAsync(original);

            var actual = await Assert.ThrowsAsync<TaskApiException>(() => service.Get(7));

            Assert.Same(original, actual);
        }
    }
}