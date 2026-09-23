using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Entities.Entity;

namespace TaskAPI.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }
        public TaskDto Create(TaskCreateDto dto)
        {
            var task = new TaskAPI.Entities.Entity.Task(dto.Title, dto.Description, DateTime.UtcNow, dto.DueDate);

            _taskRepository.Add(task);
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isCompleted(),
            };
        }

        public bool Delete(int id)
        {
            var task = _taskRepository.Get(id);
            if (task == null)
            {
                return false;
            }
            _taskRepository.Delete(task);
            return true;
        }



        public List<TaskDto> GetAll()
        {
            var tasks = _taskRepository.GetAll();
            return tasks.Select(task => new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isCompleted(),
            }).ToList();
        }

        public TaskDto Get(int id)
        {
            var task = _taskRepository.Get(id);
            if (task == null)
            {
                return null;
            }
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isCompleted(),
            };
        }

        public TaskDto Update(int id, TaskUpdateDto dto)
        {
            var task = _taskRepository.Get(id);
            if (task == null)
            {
                return null;
            }
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                DueDate = task.DueDate,
                IsCompleted = task.isCompleted(),
            };
        }
    }
}
