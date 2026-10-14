using TaskAPI.Domain.Entity;

namespace TaskAPI.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsername(string username);
        System.Threading.Tasks.Task AddAsync(User user);
        Task<User?> GetById(int id);
        Task<User?> GetByRefreshTokenHash(string tokenHash);
        Task<bool> SetRefreshToken(int userId, string tokenHash, DateTime expireAt);
        Task<bool> RotateRefreshToken(int userId, string oldTokenHash, string newTokenHash, DateTime now);
        System.Threading.Tasks.Task ClearRefreshToken(string tokenHash);
    }
}
