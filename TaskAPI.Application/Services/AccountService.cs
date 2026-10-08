using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Helpers;
namespace TaskAPI.Application.Services
{
    public class AccountService(IUserRepository userRepository, IPasswordHasher passwordHasher) : IAccountService //dependency injection for IUserRepository
    {
        private const string DummyHash = "v1.600000.AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        private IHttpContextAccessor httpContextAccessor;
        private ILogger logger;

        public async Task<bool> Register(LoginDto loginDto)
        {
            return await MethodExecutor.ExecuteAsync(async () =>
            {
                var existingUser = await userRepository.GetByUsernameAsync(loginDto.Username);
                if (existingUser != null)
                {
                    return false; // User already exists
                }

                var passwordHash = passwordHasher.Hash(loginDto.Password);
                var user = new TaskAPI.Domain.Entity.User(loginDto.Username, passwordHash, DateTime.UtcNow);
                await userRepository.AddAsync(user);
                return true; // User registered successfully
            }, logger, httpContextAccessor, "Register");
            // burda tek tek tanımlama useri direkt altına yaz
        }
        public async Task<bool> Login(LoginDto loginDto) // TODO: Implement JWT token generation and return it to the client for authentication
        {
            TaskAPI.Domain.Entity.User user = await userRepository.GetByUsernameAsync(loginDto.Username) ?? throw new Exception("Invalid username or password.");

            bool result = passwordHasher.Verify(loginDto.Password, user.PasswordHash);
            if (!result)
            {
                throw new Exception("Invalid username or password.");
            }

            // If the password is correct, you can generate a JWT token here and return it to the client for authentication.

            return true;
        }
    }
}

