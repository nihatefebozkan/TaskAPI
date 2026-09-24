using Microsoft.Extensions.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Core.Helpers;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Entities.Enums;

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
        public async Task<LoginResultEnum> LoginAsync(LoginDto loginDto)
        {
            var user = await userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                return LoginResultEnum.UserNotFound; // User not found
            }

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash); // Verify the password using BCrypt
            return isPasswordValid ? LoginResultEnum.Successfuly : LoginResultEnum.InvalidPassword; // Return the appropriate result
        }
    }
}
