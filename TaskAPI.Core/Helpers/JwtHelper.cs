using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace TaskAPI.Core.Helpers
{
    public class JwtHelper(
        IConfiguration configuration,
        ILogger<JwtHelper> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        public string CreateRefreshToken()
        {
            return MethodExecutor.Execute<string>(
                () =>
                {
                    return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));

                }, logger, httpContextAccessor, nameof(CreateRefreshToken));
        }

        public string HashRefreshToken(string refreshToken)
        {
            return MethodExecutor.Execute<string>(
                () =>
                {
                    if (string.IsNullOrWhiteSpace(refreshToken) || refreshToken.Length != 86 || refreshToken.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_'))
                        throw new TaskApiException(ErrorCodes.BadRequest, "Invalid refresh token format.", 400);

                    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

                }, logger, httpContextAccessor, nameof(HashRefreshToken));
        }

        public (string AccessToken, DateTime ExpireAt) CreateAccessToken(int userId)
        {
            return MethodExecutor.Execute<(string AccessToken, DateTime ExpireAt)>(
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
                            new Claim("sub", userId.ToString(CultureInfo.InvariantCulture)),
                            new Claim("jti", Guid.NewGuid().ToString("N")),
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

                    return (AccessToken: token, ExpireAt: expireAt);
                }, logger, httpContextAccessor, nameof(CreateAccessToken));
        }
    }
}
