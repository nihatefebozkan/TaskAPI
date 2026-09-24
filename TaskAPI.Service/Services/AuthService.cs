using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;

namespace TaskAPI.Service.Services
{
    public class AuthService(IUserRepository userRepository) : IAuthService //dependency injection for IUserRepository
    {
        public async Task<bool> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await userRepository.GetByUsernameAsync(registerDto.Username);
            if (existingUser != null)
            {
                return false; // User already exists
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password); // Hash the password using BCrypt
            var user = new TaskAPI.Entities.Entity.User(registerDto.Username, passwordHash, DateTime.UtcNow);
            {
                await userRepository.AddAsync(user);
                return true; // User registered successfully

            }
        }
    }
}
