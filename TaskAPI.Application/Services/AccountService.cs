using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Helpers;

namespace TaskAPI.Application.Services
{
    public class AccountService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<AccountService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration) : IAccountService //dependency injection for IUserRepository
    {
        private const string DummyHash =
            "v1.600000.AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        public async Task<bool> Register(LoginRequestDto loginRequestDto)
        {
            return await MethodExecutor.ExecuteAsync(async () =>
            {
                if (string.IsNullOrEmpty(loginRequestDto.Username) || string.IsNullOrEmpty(loginRequestDto.Password))
                {
                    throw new TaskApiException(ErrorCodes.BadRequest, "Username and password are required.", 400);
                }

                var existingUser = await userRepository.GetByUsername(loginRequestDto.Username);
                if (existingUser != null)
                {
                    return false; // User already exists
                }

                var passwordHash = passwordHasher.Hash(loginRequestDto.Password);
                if (string.IsNullOrWhiteSpace(passwordHash))
                {
                    throw new TaskApiException(ErrorCodes.InternalServerError, "Internal Server Error");
                }
                var user = new TaskAPI.Domain.Entity.User(loginRequestDto.Username, passwordHash, DateTime.UtcNow);
                await userRepository.AddAsync(user);
                return true; // User registered successfully
            }, logger, httpContextAccessor, "Register");
            // burda tek tek tanımlama useri direkt altına yaz
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto) // TODO: Implement JWT token generation and return it to the client for authentication
        {
            return await MethodExecutor.ExecuteAsync(
                async () =>
                {
                    var user = await userRepository.GetByUsername(loginRequestDto.Username) ??
                    throw new TaskApiException(ErrorCodes.Unauthorized, "Invalid username or password.", 401);
                    if (!passwordHasher.Verify(loginRequestDto.Password, user.PasswordHash))
                    {
                        throw new TaskApiException(ErrorCodes.Unauthorized, "Invalid username or password.", 401);
                    }

                    return CreateAccessToken(user);
                }, logger, httpContextAccessor, nameof(Login));
        }

        public LoginResponseDto CreateAccessToken(TaskAPI.Domain.Entity.User user)
        {
            return MethodExecutor.Execute<LoginResponseDto>(
                () =>
                {
                    var issuer = configuration["Jwt:Issuer"];
                    var audience = configuration["Jwt:Audience"];
                    var lifeTimeSeconds = configuration.GetValue<int>("Jwt:AccessTokenLifeTimeSeconds");
                    if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || lifeTimeSeconds <= 0)
                    {
                        throw new TaskApiException(ErrorCodes.InternalServerError, "Failed to generate access token.");
                    }
                    var expireAt = DateTime.UtcNow.AddSeconds(lifeTimeSeconds);
                    var claims = new[]
                        {
                            new Claim("sub", user.Id.ToString(CultureInfo.InvariantCulture)),
                        };
                    var signingKeyBase64 = configuration["Jwt:SigningKey"];
                    if (string.IsNullOrEmpty(signingKeyBase64))
                    {
                        throw new TaskApiException(ErrorCodes.InternalServerError, "Failed to generate access token.");
                    }
                    var keyBytes = Convert.FromBase64String(signingKeyBase64);
                    if (keyBytes.Length < 32)
                    {
                        throw new TaskApiException(ErrorCodes.InternalServerError, "Failed to generate access token.");
                    }
                    var securityKey = new SymmetricSecurityKey(keyBytes);
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var jwt = new JwtSecurityToken(
                        issuer: issuer,
                        audience: audience,
                        claims: claims,
                        expires: expireAt,
                        signingCredentials: credentials);

                    var token = new JwtSecurityTokenHandler().WriteToken(jwt);

                    return new LoginResponseDto()
                    {
                        AccessToken = token,
                        ExpireAt = expireAt
                    };
                }, logger, httpContextAccessor, nameof(CreateAccessToken));
        }
    }
}


