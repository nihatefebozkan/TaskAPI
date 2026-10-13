using TaskAPI.Domain.Entity;

namespace TaskAPI.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsername(string username);
        System.Threading.Tasks.Task AddAsync(User user);
        Task<User?> GetById(int id);
    }
}
