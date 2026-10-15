using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using System.Security.Claims;

namespace Part_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ProfileController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            var user = await _context.UserAccounts
                .Where(u => u.UserAccountID == id)
                .Select(u => new
                {
                    u.UserAccountID,
                    u.FullName,
                    u.Email,
                    u.Role
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("Profile not found.");

            return Ok(user);
        }

        // PUT: api/Profile
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(
                    "Full name and email are required.");
            }

            string email =
                request.Email.Trim().ToLowerInvariant();

            bool emailExists = await _context.UserAccounts
                .AnyAsync(u =>
                    u.Email == email &&
                    u.UserAccountID != id);

            if (emailExists)
                return BadRequest("That email is already in use.");

            var user = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.UserAccountID == id);

            if (user == null)
                return NotFound("Profile not found.");

            user.FullName = request.FullName.Trim();
            user.Email = email;

            // Do not allow users to change their own role.
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
    
