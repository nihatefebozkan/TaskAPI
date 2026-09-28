using Microsoft.Extensions.Validation;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Core.Helpers;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Entities.Enums;

namespace TaskAPI.Service.Services
{
    public class LoginService(IUserRepository userRepository, IPasswordHasher passwordHasher) : IAuthService //dependency injection for IUserRepository
    {
        private const string DummyHash = "v1.600000.AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        public async Task<bool> Register(RegisterDto registerDto)
        {
            var existingUser = await userRepository.GetByUsernameAsync(registerDto.Username);
            if (existingUser != null)
            {
                return false; // User already exists
            }

            var passwordHash = passwordHasher.Hash(registerDto.Password);
            var user = new TaskAPI.Entities.Entity.User(registerDto.Username, passwordHash, DateTime.UtcNow);
            {
                await userRepository.AddAsync(user);
                return true; // User registered successfully

            }
        }
        public async Task<LoginResponseDto> Login(LoginDto loginDto) //aut
        {
            var user = await userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
            {
                passwordHasher.Verify(loginDto.Password, DummyHash);
                return new LoginResponseDto { Result = LoginResultEnum.UserNotFound };
            }

            var isPasswordValid = passwordHasher.Verify(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new LoginResponseDto { Result = LoginResultEnum.InvalidPassword };
            }

            return new LoginResponseDto
            {
                Result = LoginResultEnum.Successfuly,
                Username = user.Username,
            };
        }
    }
}
