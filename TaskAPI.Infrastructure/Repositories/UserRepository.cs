using Microsoft.EntityFrameworkCore;
using TaskAPI.Application.Interfaces;
using TaskAPI.Domain.Entity;
using TaskAPI.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace TaskAPI.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task AddAsync(TaskAPI.Domain.Entity.User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        public async Task<Domain.Entity.User?> GetByUsername(string username)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetById(int id)
        {
            return await context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<User?> GetByRefreshTokenHash(string tokenHash)
        {
            return await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.RefreshTokenHash == tokenHash);
        }

        public async Task<bool> SetRefreshToken(int userId, string tokenHash, DateTime expireAt)
        {
            var affectedRows = await context.Users
                .Where(user => user.Id == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(user => user.RefreshTokenHash, tokenHash)
                    .SetProperty(user => user.RefreshTokenExpireAt, (DateTime?)expireAt));

            return affectedRows == 1;
        }

        public async Task<bool> RotateRefreshToken(int userId, string oldTokenHash, string newTokenHash, DateTime now)
        {
            var affectedRows = await context.Users
                .Where(user => user.Id == userId && user.RefreshTokenHash == oldTokenHash && user.RefreshTokenExpireAt > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(user => user.RefreshTokenHash, newTokenHash));

            return affectedRows == 1;
        }

        public async Task ClearRefreshToken(string tokenHash)
        {
            await context.Users
                .Where(user => user.RefreshTokenHash == tokenHash)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(user => user.RefreshTokenHash, (string?)null)
                    .SetProperty(user => user.RefreshTokenExpireAt, (DateTime?)null));
        }
    }
}
