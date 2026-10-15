using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using Part_2.Models;
using System.Security.Claims;

namespace Part_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            var user = await _context.UserAccounts
                .FindAsync(id);

            if (user == null)
                return Unauthorized();

            // Administrators and organisers can view all enrolments.
            if (user.Role == "Admin" || user.Role == "Organizer")
            {
                var all = await _context.Registrations
                    .ToListAsync();

                return Ok(all);
            }

            // Participants see only their own registrations.
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantsEmail == user.Email);

            if (participant == null)
                return Ok(new List<Registration>());

            var registrations = await _context.Registrations
                .Where(r => r.ParticipantID == participant.ParticipantID)
                .ToListAsync();

            return Ok(registrations);
        }

        // POST: api/Enrolments
        [HttpPost]
        public async Task<IActionResult> Enrol(
            EnrolmentRequest request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int id))
                return Unauthorized();

            var user = await _context.UserAccounts.FindAsync(id);

            if (user == null)
                return Unauthorized();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantsEmail == user.Email);

            if (participant == null)
            {
                return BadRequest(
                    "Create a participant record using the same email first.");
            }

            var raceEvent = await _context.RaceEvents
                .FindAsync(request.EventID);

            if (raceEvent == null)
                return NotFound("Event not found.");

            bool alreadyEnrolled = await _context.Registrations
                .AnyAsync(r =>
                    r.EventID == request.EventID &&
                    r.ParticipantID == participant.ParticipantID);

            if (alreadyEnrolled)
                return BadRequest("You are already enrolled in this event.");

            var registration = new Registration
            {
                EventID = request.EventID,
                ParticipantID = participant.ParticipantID,
                RegistrationDate = DateTime.Today,
                PaymentStatus = "Pending"
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment successful.",
                registration
            });
        }

        // DELETE: api/Enrolments/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int accountId))
                return Unauthorized();

            var user = await _context.UserAccounts
                .FindAsync(accountId);

            if (user == null)
                return Unauthorized();

            var registration = await _context.Registrations
                .FindAsync(id);

            if (registration == null)
                return NotFound("Enrolment not found.");

            if (user.Role != "Admin" && user.Role != "Organizer")
            {
                var participant = await _context.Participants
                    .FirstOrDefaultAsync(p => p.ParticipantsEmail == user.Email);

                if (participant == null ||
                    registration.ParticipantID != participant.ParticipantID)
                {
                    return Forbid();
                }
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class EnrolmentRequest
    {
        public int EventID { get; set; }
    }
}
