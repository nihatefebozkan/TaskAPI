using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Infrastructure.Data;

namespace TaskAPI.Infrastructure.Repositories
{ 
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task AddAsync(TaskAPI.Entities.Entity.User user)
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        public async Task<Entities.Entity.User?> GetByUsernameAsync(string username)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
