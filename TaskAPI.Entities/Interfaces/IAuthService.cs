using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Enums;

namespace TaskAPI.Entities.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);
    }
}   
