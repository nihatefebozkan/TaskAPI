using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Helpers;

namespace TaskAPI.Application.Services
{
    public class TaskService(ITaskRepository taskRepository, IMapper mapper, ILogger<TaskService> logger, IHttpContextAccessor httpContextAccessor) : ITaskService
    {
        public async Task<TaskDto> Add(TaskDto dto)
        {
            return await MethodExecutor.ExecuteAsync<TaskDto>(
                async () =>
                {
                    if (string.IsNullOrWhiteSpace(dto.Title))
                        throw new TaskApiException(ErrorCodes.InvalidTaskFormat, "Title cannot be empty.", 400);
                    if (dto.DueDate < DateTime.UtcNow)
                        throw new TaskApiException(ErrorCodes.InvalidTaskFormat, "Due date cannot be in the past.", 400);

                    var task = mapper.Map<TaskAPI.Domain.Entity.Task>(dto) ?? throw new TaskApiException(ErrorCodes.InternalServerError, "Task could not be mapped to domain entity", 500);

                    await taskRepository.Add(task);

                    return mapper.Map<TaskDto>(task) ?? throw new TaskApiException(ErrorCodes.InternalServerError, "Task could not be mapped to DTO", 500);

                }, logger, httpContextAccessor, nameof(Add));
        }

        public async Task<List<TaskDto>> GetAll()
        {
            return await MethodExecutor.ExecuteAsync<List<TaskDto>>(
            async () =>
            {
                var tasks = await taskRepository.GetAll() ?? [];

                return mapper.Map<List<TaskDto>>(tasks) ?? throw new TaskApiException(ErrorCodes.InternalServerError, "Tasks could not be mapped", 500);
            }, logger, httpContextAccessor, nameof(GetAll));
        }

        public async Task<TaskDto?> Get(int id)
        {
            return await MethodExecutor.ExecuteAsync<TaskDto?>(
                async () =>
                {
                    //throw new TaskApiException(ErrorCodes.InternalServerError, "Task could not be mapped");
                    var task = await taskRepository.Get(id) ?? throw new TaskApiException(ErrorCodes.TaskNotFound, "Task not found.", 404);

                    return mapper.Map<TaskDto>(task) ?? throw new TaskApiException(ErrorCodes.InternalServerError, "Task could not be mapped");
                }, logger, httpContextAccessor, nameof(Get));
        }

        public async Task<TaskDto> Update(int id, TaskDto dto)
        {
            return await MethodExecutor.ExecuteAsync<TaskDto>(
                async () =>
                  {
                      var task = await taskRepository.Get(id) ?? throw new TaskApiException(ErrorCodes.TaskNotFound, "Task not found.", 404);

                      if (dto.DueDate < DateTime.UtcNow)
                          throw new TaskApiException(ErrorCodes.InvalidTaskFormat, "Due date cannot be in the past.", 400);

                      task.DueDate = dto.DueDate;

                      await taskRepository.Update(task);

                      return mapper.Map<TaskDto>(task) ?? throw new TaskApiException(ErrorCodes.InternalServerError, "Task could not be mapped", 500);
                  }, logger, httpContextAccessor, nameof(Update));
        }
        public async Task<bool> Delete(int id)
        {
            return await MethodExecutor.ExecuteAsync<bool>(
                async () =>
                {
                    var task = await taskRepository.Get(id) ?? throw new TaskApiException(ErrorCodes.TaskNotFound, "Task not found.", 404);

                    task.DeletedAt = DateTimeOffset.UtcNow;

                    await taskRepository.Delete(task);

                    return true;
                }, logger, httpContextAccessor, nameof(Delete));
        }
    }
}
