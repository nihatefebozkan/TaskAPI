using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Application.Dtos;

namespace TaskAPI.Application.Interfaces
{
    public interface IAccountService 
    {
        Task<bool> Register(LoginDto loginDto);
        Task<bool> Login(LoginDto loginDto);
    }
}   
