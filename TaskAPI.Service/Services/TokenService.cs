using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using TaskAPI.Entities.Entity;
using TaskAPI.Entities.Interfaces;

namespace TaskAPI.Service.Services
{
    public class TokenService(IConfiguration configuration) : ITokenService
    {
        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:key"!]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(configuration["Jwt:ExpiryMinutes"])),
                signingCredentials: credentials);       
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
