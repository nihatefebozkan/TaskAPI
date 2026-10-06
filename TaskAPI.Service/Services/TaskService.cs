using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Helpers;
using TaskAPI.Domain.Entity;

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
                        throw new ArgumentException("Title cannot be empty.", "task");
                    if (dto.DueDate < DateTime.UtcNow)
                        throw new ArgumentException("Due date cannot be in the past.", "task");

                    var task = mapper.Map<TaskAPI.Domain.Entity.Task>(dto) ?? throw new ArgumentException("Task could not be mapped to domain entity");

                    await taskRepository.Add(task);

                    return mapper.Map<TaskDto>(task) ?? throw new ArgumentException("Task could not be mapped to DTO");

                }, logger, httpContextAccessor, nameof(Add));
        }

        public async Task<List<TaskDto>> GetAll()
        {
            return await MethodExecutor.ExecuteAsync<List<TaskDto>>(
                async () =>
                {
                    var tasks = await taskRepository.GetAll() ?? [];

                    return mapper.Map<List<TaskDto>>(tasks) ?? throw new ArgumentException("Tasks could not be mapped");
                },
                logger,
                httpContextAccessor,
                nameof(GetAll));
        }

        public async Task<TaskDto?> Get(int id)
        {
            return await MethodExecutor.ExecuteAsync<TaskDto?>(
                async () =>
                {
                    var task = await taskRepository.Get(id) ?? throw new ArgumentException("Task not found.");

                    return mapper.Map<TaskDto>(task) ?? throw new ArgumentException("Task could not be MAPPED");
                },
                logger,
                httpContextAccessor,
                nameof(Get));
        }

        public async Task<TaskDto> Update(int id, TaskDto dto)
        {
            return await MethodExecutor.ExecuteAsync<TaskDto>(
                async () =>
                {
                    var task = await taskRepository.Get(id) ?? throw new ArgumentException("Task not found.");

                    if (dto.DueDate < DateTime.UtcNow)
                        throw new ArgumentException("Due date cannot be in the past.", "task");
                    
                    task.DueDate = dto.DueDate;

                    await taskRepository.Update(task);

                    return mapper.Map<TaskDto>(task) ?? throw new ArgumentException("Task could not be mapped");
                },
                logger,
                httpContextAccessor,
                nameof(Update));
        }
        public async Task<bool> Delete(int id)
        {
            return await MethodExecutor.ExecuteAsync<bool>(
                async () =>
                {
                    var task = await taskRepository.Get(id) ?? throw new ArgumentException("Task not found");

                    task.DeletedAt = DateTimeOffset.UtcNow;

                    await taskRepository.Delete(task);

                    return true;
                },
                logger,
                httpContextAccessor,
                nameof(Delete));
        }
    }
}
