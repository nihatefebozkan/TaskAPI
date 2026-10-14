using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        IConfiguration configuration,
        JwtHelper jwtHelper) : IAccountService //dependency injection for IUserRepository
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

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            return await MethodExecutor.ExecuteAsync<LoginResponseDto>(
                async () =>
                {
                    var user = await userRepository.GetByUsername(loginRequestDto.Username) ??
                    throw new TaskApiException(ErrorCodes.Unauthorized, "Invalid username or password.", 401);
                    if (!passwordHasher.Verify(loginRequestDto.Password, user.PasswordHash))
                    {
                        throw new TaskApiException(ErrorCodes.Unauthorized, "Invalid username or password.", 401);
                    }

                    var refreshTokenLifeTimeSeconds = configuration.GetValue<int>("Jwt:RefreshTokenLifeTimeSeconds");

                    if (refreshTokenLifeTimeSeconds <= 0)
                        throw new TaskApiException(ErrorCodes.InternalServerError, "Refresh token configuration is invalid.", 500);

                    var accessToken = jwtHelper.CreateAccessToken(user.Id);
                    var response = new LoginResponseDto
                    {
                        AccessToken = accessToken.AccessToken,
                        ExpireAt = accessToken.ExpireAt
                    };
                    var refreshToken = jwtHelper.CreateRefreshToken();
                    var refreshTokenExpireAt = DateTime.UtcNow.AddSeconds(refreshTokenLifeTimeSeconds);

                    var saved = await userRepository.SetRefreshToken(user.Id, jwtHelper.HashRefreshToken(refreshToken), refreshTokenExpireAt);

                    if (!saved)
                        throw new TaskApiException(ErrorCodes.Unauthorized, "User no longer exists.", 401);

                    response.RefreshToken = refreshToken;
                    response.RefreshTokenExpireAt = refreshTokenExpireAt;

                    return response;
                }, logger, httpContextAccessor, nameof(Login));
        }

        public async Task<LoginResponseDto> Refresh(string refreshToken)
        {
            return await MethodExecutor.ExecuteAsync<LoginResponseDto>(
                async () =>
                {
                    var tokenHash = jwtHelper.HashRefreshToken(refreshToken);
                    var user = await userRepository.GetByRefreshTokenHash(tokenHash);
                    var now = DateTime.UtcNow;

                    if (user is null || user.RefreshTokenExpireAt is null || user.RefreshTokenExpireAt <= now)
                        throw new TaskApiException(ErrorCodes.Unauthorized, "Refresh token is invalid or expired.", 401);

                    var accessToken = jwtHelper.CreateAccessToken(user.Id);
                    var response = new LoginResponseDto
                    {
                        AccessToken = accessToken.AccessToken,
                        ExpireAt = accessToken.ExpireAt
                    };
                    var newRefreshToken = jwtHelper.CreateRefreshToken();

                    var rotated = await userRepository.RotateRefreshToken(user.Id, tokenHash, jwtHelper.HashRefreshToken(newRefreshToken), now);

                    if (!rotated)
                        throw new TaskApiException(ErrorCodes.Unauthorized, "Refresh token is invalid or expired.", 401);

                    response.RefreshToken = newRefreshToken;
                    response.RefreshTokenExpireAt = DateTime.SpecifyKind(user.RefreshTokenExpireAt.Value, DateTimeKind.Utc);

                    return response;

                }, logger, httpContextAccessor, nameof(Refresh));
        }

        public async Task<bool> Logout(string refreshToken)
        {
            return await MethodExecutor.ExecuteAsync<bool>(
                async () =>
                {
                    var tokenHash = jwtHelper.HashRefreshToken(refreshToken);

                    await userRepository.ClearRefreshToken(tokenHash);

                    return true;

                }, logger, httpContextAccessor, nameof(Logout));
        }

    }
}
