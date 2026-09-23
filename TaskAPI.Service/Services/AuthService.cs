using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;

namespace TaskAPI.Service.Services
{
    public class AuthService(IUserRepository userRepository) : IAuthService //dependency injection for IUserRepository
    {
        public bool Register(RegisterDto registerDto)
        {
            var existingUser = userRepository.GetByUsername(registerDto.Username);
            if (existingUser != null)
            {
                return false; // User already exists
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password); // Hash the password using BCrypt
            var user = new TaskAPI.Entities.Entity.User(registerDto.Username, passwordHash, DateTime.UtcNow);
            {
                userRepository.Add(user);
                return true; // User registered successfully

            }
        }
    }
}
