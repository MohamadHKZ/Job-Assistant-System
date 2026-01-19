using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.DTOs;
using API.Entities;
using JobAssistantSystem.API.Interfaces;
using Humanizer;
using Microsoft.IdentityModel.Tokens;

namespace JobAssistantSystem.API.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        public string GenerateToken(User user)
        {
            var token = new JwtSecurityToken(
                expires: DateTime.Now.AddDays(7),
                claims:
                [
                    new Claim(JwtRegisteredClaimNames.NameId, user.UserId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)
                ],
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super secret key super secret key")),
                    SecurityAlgorithms.HmacSha256Signature
                ));
            var jwtHandler = new JwtSecurityTokenHandler();
            var tokenString = jwtHandler.WriteToken(token);
            return tokenString;
        }
    }
}