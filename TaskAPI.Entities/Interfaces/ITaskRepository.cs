using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Entity;

namespace TaskAPI.Entities.Interfaces
{
    public interface ITaskRepository
    {
        List<TaskAPI.Entities.Entity.Task> GetAll();
        TaskAPI.Entities.Entity.Task Get(int id);
        void Add(TaskAPI.Entities.Entity.Task task);
        void Update(TaskAPI.Entities.Entity.Task task);
        void Delete(TaskAPI.Entities.Entity.Task task);
    }
}
