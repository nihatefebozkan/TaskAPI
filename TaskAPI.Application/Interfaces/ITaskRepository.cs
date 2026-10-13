namespace TaskAPI.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task<List<TaskAPI.Domain.Entity.Task>> GetAll(int userId);
        Task<Domain.Entity.Task?> Get(int id, int userId);
        Task<Domain.Entity.Task> Add(Domain.Entity.Task task);
        Task<Domain.Entity.Task> Update(Domain.Entity.Task task);
        Task<Domain.Entity.Task> Delete(Domain.Entity.Task task);
    }
}
