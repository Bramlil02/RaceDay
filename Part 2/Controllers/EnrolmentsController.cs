using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using Part_2.Models;
using System.Security.Claims;

namespace Part_2.Controllers
{//Start of namespace

    // All enrolment endpoints require a valid login.
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {// Start of class

        // Provides access to accounts, participants and registrations.
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {// Start of constructor

            // Assigns the database context to a private field.
            _context = context;

        }// End of constructor


        // GET: api/Enrolments
        // Participants see their own enrolments.
        // Admins and organisers can see all enrolments.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {// Start of GetEnrolments method

            // Gets the account ID from the JWT token
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // If the account ID is not a valid integer, return Unauthorized.
            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            // Find the user account in the database using the account ID.
            var user = await _context.UserAccounts
                .FindAsync(id);


            // If the user account is not found, return Unauthorized.
            if (user == null)
                return Unauthorized();

            // Administrators and organisers can view all enrolments.
            if (user.Role == "Admin" || user.Role == "Organizer")
            {// Start of if block

                // Get all registrations from the database.
                var all = await _context.Registrations
                    .ToListAsync();

                // Return the list of all registrations.
                return Ok(all);

            }// End of if block

            // Participants see only their own registrations.
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantsEmail == user.Email);

            // If the participant record is not found, return an empty list.
            if (participant == null)
                return Ok(new List<Registration>());

            // Get all registrations for the participant from the database.
            var registrations = await _context.Registrations
                .Where(r => r.ParticipantID == participant.ParticipantID)
                .ToListAsync();

            // Return the list of registrations for the participant.
            return Ok(registrations);
        }// End of GetEnrolments method

        // POST: api/Enrolments
        // Enrols the logged-in participant in an event.
        [HttpPost]
        public async Task<IActionResult> Enrol(
            EnrolmentRequest request)
        {// Start of Enrol method

            // Get the account ID from the JWT token
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // If the account ID is not a valid integer, return Unauthorized.
            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            // Find the user account in the database using the account ID.
            var user = await _context.UserAccounts.FindAsync(id);

            // If the user account is not found, return Unauthorized.
            if (user == null)
                return Unauthorized();
            
            // Find the participant record in the database using the user's email.
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantsEmail == user.Email);

            // If the participant record is not found, return BadRequest.
            if (participant == null)
            {// Start of if block
                return BadRequest(
                    "Create a participant record using the same email first.");
            }// End of if block

            // Find the race event in the database using the event ID from the request.
            var raceEvent = await _context.RaceEvents
                .FindAsync(request.EventID);

            // If the race event is not found, return NotFound.
            if (raceEvent == null)
                return NotFound("Event not found.");

            // Check if the participant is already enrolled in the event.
            bool alreadyEnrolled = await _context.Registrations
                .AnyAsync(r =>
                    r.EventID == request.EventID &&
                    r.ParticipantID == participant.ParticipantID);

            // If the participant is already enrolled, return BadRequest.
            if (alreadyEnrolled)
                return BadRequest("You are already enrolled in this event.");


            // Create a new registration object and set its properties.
            var registration = new Registration
            {// Start of registration object initialization
                EventID = request.EventID,
                ParticipantID = participant.ParticipantID,
                RegistrationDate = DateTime.Today,
                PaymentStatus = "Pending"
            };// End of registration object initialization

            // Add the new registration to the database context and save changes.
            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            // Return a success message along with the registration details.
            return Ok(new
            {// Start of anonymous object initialization

                message = "Enrolment successful.",
                registration
            });// End of anonymous object initialization
        }// End of Enrol method

        // DELETE: api/Enrolments/1
        // Cancels an enrolment using its RegistrationID.
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {// Start of CancelEnrolment method

            // Get the account ID from the JWT token
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            // If the account ID is not a valid integer, return Unauthorized.
            if (!int.TryParse(userId, out int accountId))
                return Unauthorized();
            
            // Find the user account in the database using the account ID.
            var user = await _context.UserAccounts
                .FindAsync(accountId);


            // If the user account is not found, return Unauthorized.
            if (user == null)
                return Unauthorized();

            // Find the registration record in the database using the RegistrationID.
            var registration = await _context.Registrations
                .FindAsync(id);

            // If the registration record is not found, return NotFound.
            if (registration == null)
                return NotFound("Enrolment not found.");

            // Check if the user is not an Admin or Organizer.
            if (user.Role != "Admin" && user.Role != "Organizer")
            {// Start of if block

                // Find the participant record in the database using the user's email.
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.ParticipantsEmail == user.Email);

                // If the participant record is not found or the registration does not belong to the participant, return Forbid.
                if (participant == null ||
                    registration.ParticipantID != participant.ParticipantID)
                {// Start of if block

                    return Forbid();
                }// End of if block
            }// End of if block

            // Remove the registration from the database context and save changes.
            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            // Return NoContent to indicate successful deletion without returning any content.
            return NoContent();
        }// End of CancelEnrolment method
    }// End of class

    public class EnrolmentRequest
    {// Start of EnrolmentRequest class

        // The ID of the event for which the participant wants to enrol.
        public int EventID { get; set; }

    }// End of EnrolmentRequest class
}// End of namespace
