using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Entities.Entity;
using TaskAPI.Core.Helpers;

namespace TaskAPI.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }
        public async Task<TaskDto> AddAsync(TaskCreateDto dto)
        {
            var task = new TaskAPI.Entities.Entity.Task(dto.Title, dto.Description, DateTime.UtcNow, dto.DueDate);

            await _taskRepository.AddAsync(task);
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isOverdue(),
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _taskRepository.GetAsync(id);
            if (task == null)
            {
                return false;
            }
            await _taskRepository.DeleteAsync(task);
            return true;
        }



        public async Task<List<TaskDto>> GetAllAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return tasks.Select(task => new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isOverdue(),
            }).ToList();
        }

        public async Task<TaskDto> GetAsync(int id)
        {
            //throw new Exception("Test Hatası");
            var task = await _taskRepository.GetAsync(id);
            if (task == null)
            {
                throw new NotFoundException("Task Not Found");
            }
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isOverdue(),
            };
        }

        public async Task<TaskDto> UpdateAsync(int id, TaskUpdateDto dto)
        {
            var task = await _taskRepository.GetAsync(id);
            if (task == null)
            {
                return null;
            }
            //var task = new Entities.Entity.Task(dto.Title, dto.Description, task.CreatedAt, dto.DueDate);
            task.Title = dto.Title;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;
            await _taskRepository.UpdateAsync(task);
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isOverdue(),
            };
        }
    }
}
