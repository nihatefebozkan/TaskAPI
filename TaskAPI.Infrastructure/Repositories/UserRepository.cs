using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Infrastructure.Data;

namespace TaskAPI.Infrastructure.Repositories
{ 
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public void Add(TaskAPI.Entities.Entity.User user)
        {
            context.Users.Add(user);
            context.SaveChanges();
        }
        public TaskAPI.Entities.Entity.User? GetByUsername(string username)
        {
            return context.Users.FirstOrDefault(u => u.Username == username);
        }
    }
}
