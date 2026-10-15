using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Part_2.Data;
using Part_2.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Part_2.DTOs;

namespace Part_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IPasswordHasher<UserAccount> _hasher;
        private readonly IConfiguration _configuration;

        public AuthController(
            RaceDayContext context,
            IPasswordHasher<UserAccount> hasher,
            IConfiguration configuration)
        {
            _context = context;
            _hasher = hasher;
            _configuration = configuration;
        }

        // POST: api/Auth/register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(
            DTOs.RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(
                    "Full name, email and password are required.");
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(
                    "Password must be at least 8 characters.");
            }

            string email = request.Email.Trim().ToLowerInvariant();

            bool exists = await _context.UserAccounts
                .AnyAsync(u => u.Email == email);

            if (exists)
            {
                return BadRequest(
                    "An account with this email already exists.");
            }

            // Users cannot choose their own privileged role.
            var user = new UserAccount
            {
                FullName = request.Name.Trim(),
                Email = email,
                Role = "Participant"
            };

            user.PasswordHash =
                _hasher.HashPassword(user, request.Password);

            _context.UserAccounts.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Account registered successfully."
            });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            DTOs.LoginRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();

            var user = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            var result = _hasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(
                    "Invalid email or password.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,
                    user.UserAccountID.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            string jwtKey =
                _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is missing.");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return Ok(new
            {
                message = "Login successful.",
                token = new JwtSecurityTokenHandler()
                    .WriteToken(token),
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            }
            );
        }
    }
}
    

