using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Domain.Entity;

namespace TaskAPI.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<Domain.Entity.Task>> GetAll();
        Task<Domain.Entity.Task> Get(int id);
        Task<Domain.Entity.Task> Add(Domain.Entity.Task task);
        Task<Domain.Entity.Task> Update(Domain.Entity.Task task);
        Task<Domain.Entity.Task> Delete(Domain.Entity.Task task);
    }
}
