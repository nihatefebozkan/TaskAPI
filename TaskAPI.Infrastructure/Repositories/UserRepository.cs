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
    }
}
