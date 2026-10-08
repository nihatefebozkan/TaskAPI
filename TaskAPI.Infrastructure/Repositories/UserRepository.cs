using Microsoft.EntityFrameworkCore;
using TaskAPI.Application.Interfaces;
using TaskAPI.Infrastructure.Data;

namespace TaskAPI.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task AddAsync(TaskAPI.Domain.Entity.User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        public async Task<Domain.Entity.User?> GetByUsernameAsync(string username)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
