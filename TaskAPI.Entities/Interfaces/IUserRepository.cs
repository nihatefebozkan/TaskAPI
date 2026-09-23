using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Entity;

namespace TaskAPI.Entities.Interfaces
{
    public interface IUserRepository
    {
        User? GetByUsername(string username);
        void Add(User user);
    }
}
