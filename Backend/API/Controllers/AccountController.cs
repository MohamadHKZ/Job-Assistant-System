using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using JobAssistantSystem.API.Errors;
using JobAssistantSystem.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountsController(AppDbContext _context, IJwtTokenService _jwtTokenService) : BaseController
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDto)
        {
            if (await EmailExists(registerDto.Email))
                throw new EmailAlreadyTakenException(registerDto.Email);

            using HMACSHA512 hmac = new HMACSHA512();
            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.ToUserDTO(_jwtTokenService.GenerateToken(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);
            // Single failure path (no email-vs-password leak) — prevents user enumeration.
            if (user == null) throw new InvalidCredentialsException();

            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // Constant-time comparison defends against timing attacks.
            if (!CryptographicOperations.FixedTimeEquals(computedHash, user.PasswordHash))
                throw new InvalidCredentialsException();

            return user.ToUserDTO(_jwtTokenService.GenerateToken(user));
        }

        private async Task<bool> EmailExists(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }
    }
}
