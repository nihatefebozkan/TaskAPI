using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Entity;

namespace TaskAPI.Entities.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        System.Threading.Tasks.Task AddAsync(User user);
    }
}
