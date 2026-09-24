using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Dtos;

namespace TaskAPI.Entities.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDto registerDto);

    }
}
