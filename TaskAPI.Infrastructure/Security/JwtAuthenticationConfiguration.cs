using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace TaskAPI.Infrastructure.Security
{
    public static class JwtAuthenticationConfiguration
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];
            var signingKeyBase64 = configuration["Jwt:SigningKey"];

            if (string.IsNullOrWhiteSpace(issuer) ||
                string.IsNullOrWhiteSpace(audience) ||
                string.IsNullOrWhiteSpace(signingKeyBase64))
            {
                throw new InvalidOperationException(
                    "JWT configuration is missing.");
            }

            var keyBytes = Convert.FromBase64String(signingKeyBase64);

            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT signing key must contain at least 32 bytes.");
            }

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(keyBytes),
                            ValidAlgorithms =
                                new[] { SecurityAlgorithms.HmacSha256 },

                            ValidateIssuer = true,
                            ValidIssuer = issuer,

                            ValidateAudience = true,
                            ValidAudience = audience,

                            ValidateLifetime = true,
                            RequireExpirationTime = true,
                            RequireSignedTokens = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    //options.Events = new JwtBearerEvents()
                    //{
                    //    OnChallenge = context =>
                    //    {
                    //        if (context.AuthenticateFailure is SecurityTokenExpiredException)

                    //        {
                    //            context.HandleResponse();
                    //            context.Response.StatusCode = 498; // Invalid Token
                    //        }

                    //        return Task.CompletedTask;
                    //    }
                    //};
                });

            services.AddAuthorization();

            return services;
        }
    }
}

