using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Entity;

namespace TaskAPI.Entities.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<Entity.Task>> GetAllAsync();
        Task<Entity.Task> GetAsync(int id);
        Task<Entity.Task> AddAsync(Entity.Task task);
        Task<Entity.Task> UpdateAsync(Entity.Task task);
        Task<Entity.Task> DeleteAsync(Entity.Task task);
    }
}
