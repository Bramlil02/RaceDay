using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Part_2.Controllers
{//Start of namespace

    //Defines the profile API and requires a valid login token.
   [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {// Start of class

        // Provides access to user account records
        private readonly RaceDayContext _context;

        // Receives the database context.
        public ProfileController(RaceDayContext context)
        {// Start of constructor

            // Assigns the database context to a private field.
            _context = context;
        }// End of constructor

        // GET: api/Profile
        // Returns the profile of the currently logged-in user.
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {// Start of GetProfile method

            // Reads the user's ID from the JWT token.
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // Checks that the ID is a valid integer.
            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            // Retrieves the account but excludes the password hash.
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

            // Returns an error if the account cannot be found.
            if (user == null)
                return NotFound("Profile not found.");

            // Returns the user's profile information.
            return Ok(user);
        }

        // PUT: api/Profile
        // Updates the logged-in user's name and email.
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            UpdateProfileRequest request)
        {// Start of UpdateProfile method

            // Gets the account ID from the login token.
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // Checks that the ID is a valid integer.
            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            // Checks that both required values are supplied.
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {// Start of validation check
                return BadRequest(
                    "Full name and email are required.");
            }// End of validation check

            // Standardises the email address.
            string email =
                request.Email.Trim().ToLowerInvariant();

            // Checks whether another account uses the email.
            bool emailExists = await _context.UserAccounts
                .AnyAsync(u =>
                    u.Email == email &&
                    u.UserAccountID != id);

            if (emailExists)
                return BadRequest("That email is already in use.");

            // Finds the current user's account.
            var user = await _context.UserAccounts
                .FirstOrDefaultAsync(u => u.UserAccountID == id);

            // Returns an error if the account cannot be found.
            if (user == null)
                return NotFound("Profile not found.");


            // Updates only the permitted profile fields.
            user.FullName = request.FullName.Trim();
            user.Email = email;

            // Saves the changes to the database.
            await _context.SaveChangesAsync();

            return Ok(new
            {// Start of response object

                // Returns a success message.
                message = "Profile updated successfully."
            });// End of response object
        }// End of UpdateProfile method
    }// End of class

    // Defines the fields allowed in a profile update request.
    public class UpdateProfileRequest
    {// Start of UpdateProfileRequest class

        // The new full name for the user.
        public string FullName { get; set; } = string.Empty;

        // The new email address for the user.
        public string Email { get; set; } = string.Empty;
    }// End of UpdateProfileRequest class
}// End of namespace

