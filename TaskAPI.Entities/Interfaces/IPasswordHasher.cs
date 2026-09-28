using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Entities.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string storedHash);
    }
}
