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
{//Start of namespace
    // Defines the API controller and its base URL.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {// Start of class

        // Provides access to the database.
        private readonly RaceDayContext _context;

        // Checks and creates password hashes.
        private readonly IPasswordHasher<UserAccount> _hasher;

        // Reads JWT settings from configuration.
        private readonly IConfiguration _configuration;

        // Receives the required services through dependency injection.
        public AuthController(
            RaceDayContext context,
            IPasswordHasher<UserAccount> hasher,
            IConfiguration configuration)

        {// Start of constructor
            _context = context;
            _hasher = hasher;
            _configuration = configuration;
        }// End of constructor

        // POST: api/Auth/register
        // Creates a new participant account.
        [HttpPost("Register")]
        public async Task<IActionResult> Register(
            DTOs.RegisterRequest request)

        // Checks that the required registration fields are provided.

        {// Start of Register method
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {// Start of if statement
                return BadRequest(
                    "Full name, email and password are required.");
            }// End of if statement

            // Requires a password of at least eight characters.
            if (request.Password.Length < 8)

            {// Start of if statement
                return BadRequest(
                    "Password must be at least 8 characters.");
            }// End of if statement

            // Standardises the email before checking for duplicates.
            string email = request.Email.Trim().ToLowerInvariant();

            // Checks whether an account already uses this email.
            bool exists = await _context.UserAccounts
                .AnyAsync(u => u.Email == email);

            // Prevents duplicate accounts.
            if (exists)
            {// Start of if statement

                // Returns a 400 Bad Request response with an error message.
                return BadRequest(
                    "An account with this email already exists.");


            }// End of if statement

            // Creates a new account with the Participant role.
            // Do not trust a role sent by an unregistered user.
            var user = new UserAccount
            {// Start of object initializer

                // Trims whitespace from the name and email.
                FullName = request.Name.Trim(),
                Email = email,
                Role = "Participant"

            };// End of object initializer

            // Hashes the password before storing it in the database.
            user.PasswordHash =
                _hasher.HashPassword(user, request.Password);

            // Adds the account to the database
            _context.UserAccounts.Add(user);

            // Saves the new account.
            await _context.SaveChangesAsync();

            // Returns a success response.
            return Ok(new
            {// Start of anonymous object
                message = "Account registered successfully."
            });// End of anonymous object
        }// End of Register method

        // POST: api/Auth/login
        // Checks the user's credentials and returns a token.
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            DTOs.LoginRequest request)
        {// Start of Login method
            // Standardises the email before searching for the account.
            string email = request.Email.Trim().ToLowerInvariant();

            // Finds the account with the supplied email.
            var user = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.Email == email);

            // Rejects login if the account does not exist.
            if (user == null)
            {// Start of if statement

                // Returns a 401 Unauthorized response with an error message.
                return Unauthorized(
                    "Invalid email or password.");
            }// End of if statement

            // Compares the entered password with the stored hash.
            var result = _hasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            // Rejects login if the password is incorrect.
            if (result == PasswordVerificationResult.Failed)
            {// Start of if statement

                // Returns a 401 Unauthorized response with an error message.
                return Unauthorized(
                    "Invalid email or password.");
            }// End of if statement

            // Adds the user's identity and role to the token.
            var claims = new[]
            {// Start of claims array

                // Adds the user's ID, name, email, and role to the token.
                new Claim(ClaimTypes.NameIdentifier,
                    user.UserAccountID.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };// End of claims array

            // Reads the secret used to sign the token.
            string jwtKey =
                _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT Key is missing.");

            // Creates a symmetric security key from the secret.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            // Specifies the algorithm used to sign the token.
            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            // Creates a token that expires after two hours.
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            // Returns the token and basic account information.
            return Ok(new
            {// Start of anonymous object

                // Returns a success message, the token, and user details.
                message = "Login successful.",
                token = new JwtSecurityTokenHandler()
                    .WriteToken(token),
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            }// End of anonymous object
            );
        }// End of Login method
    }// End of class
}//End of namespace


