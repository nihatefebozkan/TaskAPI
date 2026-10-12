using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Application.Services;
using TaskAPI.Core.Helpers;
using Xunit;
using User = TaskAPI.Domain.Entity.User;

namespace TaskAPI.API.Tests
{
    public class AccountServiceTests
    {
        private readonly Mock<IUserRepository> repository = new();
        private readonly Mock<IPasswordHasher> hasher = new();
        private readonly AccountService service;
        private readonly LoginRequestDto dto = new() { Username = "test-user", Password = "test-password" };
        private readonly User user = new("test-user", "stored-hash", DateTime.UtcNow);
        private readonly byte[] testSigningKey = RandomNumberGenerator.GetBytes(32);

        public AccountServiceTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "TaskAPI.Tests",
                    ["Jwt:Audience"] = "TaskAPI.Tests",
                    ["Jwt:AccessTokenLifeTimeSeconds"] = "1800",
                    ["Jwt:SigningKey"] =
                        Convert.ToBase64String(testSigningKey)
                })
                .Build();

            service = new AccountService(
                repository.Object,
                hasher.Object,
                NullLogger<AccountService>.Instance,
                TestSupport.CreateAccessor(),
                configuration);
        }
        [Fact]
        public async Task Login_CorrectPassword_ReturnsValidToken()
        {
            user.Id = 42;

            repository.Setup(repo => repo.GetByUsername(dto.Username))
                .ReturnsAsync(user);

            hasher.Setup(current =>
                    current.Verify(dto.Password, user.PasswordHash))
                .Returns(true);

            var before = DateTime.UtcNow;
            var response = await service.Login(dto);
            var after = DateTime.UtcNow;

            Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
            Assert.InRange(
                response.ExpireAt,
                before.AddSeconds(1800),
                after.AddSeconds(1800));

            var handler = new JwtSecurityTokenHandler
            {
                MapInboundClaims = false
            };

            var principal = handler.ValidateToken(
                response.AccessToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(testSigningKey),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ValidateIssuer = true,
                    ValidIssuer = "TaskAPI.Tests",
                    ValidateAudience = true,
                    ValidAudience = "TaskAPI.Tests",
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero
                },
                out var validatedToken);

            Assert.Equal("42", principal.FindFirst("sub")?.Value);

            var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);

            Assert.True(
                Math.Abs((jwt.ValidTo - response.ExpireAt).TotalSeconds) < 1);

            hasher.Verify(current =>
                    current.Verify(dto.Password, user.PasswordHash),
                Times.Once);
        }

        [Fact]
        public async Task Login_MissingUser_DoesNotVerifyPassword()
        {
            repository.Setup(repo => repo.GetByUsername(dto.Username)).ReturnsAsync((User?)null);

            var exception = await Assert.ThrowsAsync<TaskApiException>(() => service.Login(dto));

            Assert.Equal(401, exception.errorModel.StatusCode);
            Assert.Equal("Invalid username or password.", exception.Message);
            hasher.Verify(current => current.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsCurrentAuthenticationError()
        {
            repository.Setup(repo => repo.GetByUsername(dto.Username)).ReturnsAsync(user);
            hasher.Setup(current => current.Verify(dto.Password, user.PasswordHash)).Returns(false);

            var exception = await Assert.ThrowsAsync<TaskApiException>(() => service.Login(dto));

            Assert.Equal(401, exception.errorModel.StatusCode);
            Assert.Equal("Invalid username or password.", exception.Message);
        }

        [Fact]
        public async Task Login_RepositoryFailure_IsNotHidden()
        {
            var original = new InvalidOperationException("Repository failure.");
            repository.Setup(repo => repo.GetByUsername(dto.Username)).ThrowsAsync(original);

            var exception = await Assert.ThrowsAsync<TaskApiException>(
                () => service.Login(dto));

            Assert.Equal(500, exception.errorModel.StatusCode);
            Assert.Same(original, exception.InnerException);
        }

        [Fact]
        public async Task Login_HasherFailure_IsNotHidden()
        {
            var original = new FormatException("Invalid stored hash.");
            repository.Setup(repo => repo.GetByUsername(dto.Username)).ReturnsAsync(user);
            hasher.Setup(current => current.Verify(dto.Password, user.PasswordHash)).Throws(original);

            var exception = await Assert.ThrowsAsync<TaskApiException>(
                () => service.Login(dto));

            Assert.Equal(500, exception.errorModel.StatusCode);
            Assert.Same(original, exception.InnerException);
        }

        [Fact]
        public async Task Register_NewUser_ShouldHashPasswordAndSaveUser()
        {
            repository.Setup(repo => repo.GetByUsername(dto.Username)).ReturnsAsync((User?)null);
            hasher.Setup(current => current.Hash(dto.Password)).Returns("generated-hash");
            repository.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await service.Register(dto);

            Assert.True(result);
            hasher.Verify(current => current.Hash(dto.Password), Times.Once);

            repository.Verify(repo => repo.AddAsync(It.Is<User>(saved =>
                saved.Username == dto.Username &&
                saved.PasswordHash == "generated-hash" &&
                saved.PasswordHash != dto.Password)), Times.Once);
        }

        [Fact]
        public async Task Register_ExistingUser_ShouldReturnFalseWithoutSaving()
        {
            repository.Setup(repo => repo.GetByUsername(dto.Username)).ReturnsAsync(user);

            Assert.False(await service.Register(dto));

            hasher.Verify(current => current.Hash(It.IsAny<string>()), Times.Never);
            repository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}