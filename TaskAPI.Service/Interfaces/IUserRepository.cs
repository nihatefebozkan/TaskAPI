using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Domain.Entity;

namespace TaskAPI.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        System.Threading.Tasks.Task AddAsync(User user);
    }
}
