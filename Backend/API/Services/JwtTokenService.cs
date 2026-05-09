using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using JobAssistantSystem.API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace JobAssistantSystem.API.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        public string GenerateToken(User user, IEnumerable<string> roles)
        {
            var claimList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };

            foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var trimmed = role.Trim();
                if (trimmed.Length > 0)
                {
                    // Use ClaimTypes.Role so JwtBearer + [Authorize(Roles = "...")] agree after inbound mapping.
                    claimList.Add(new Claim(ClaimTypes.Role, trimmed));
                }
            }

            var token = new JwtSecurityToken(
                expires: DateTime.UtcNow.AddDays(7),
                claims: claimList,
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super secret key super secret key")),
                    SecurityAlgorithms.HmacSha256Signature
                ));
            var jwtHandler = new JwtSecurityTokenHandler();
            return jwtHandler.WriteToken(token);
        }
    }
}
