using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;

namespace Part_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Results
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Registrations
                .Where(r => r.FinishTime != null)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/Results/1
        // The ID here is the RegistrationID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Registrations
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            if (result == null)
                return NotFound("Result not found.");

            return Ok(result);
        }

        // POST: api/Results
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> RecordResult(
            RecordResultRequest request)
        {
            var registration = await _context.Registrations
                .FindAsync(request.RegistrationID);

            if (registration == null)
                return NotFound("Registration not found.");

            if (string.IsNullOrWhiteSpace(request.FinishTime))
                return BadRequest("Finish time is required.");

            registration.FinishTime = request.FinishTime;
            registration.Position = request.Position;
            registration.AveragePace = request.AveragePace;
            registration.ResultStatus = request.ResultStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result recorded successfully.",
                registration
            });
        }

        // PUT: api/Results/1
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id, RecordResultRequest request)
        {
            if (id != request.RegistrationID)
                return BadRequest("Registration ID does not match.");

            var registration = await _context.Registrations
                .FindAsync(id);

            if (registration == null)
                return NotFound("Registration not found.");

            registration.FinishTime = request.FinishTime;
            registration.Position = request.Position;
            registration.AveragePace = request.AveragePace;
            registration.ResultStatus = request.ResultStatus;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully.",
                registration
            });
        }
    }

    public class RecordResultRequest
    {
        public int RegistrationID { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public decimal? AveragePace { get; set; }
        public string? ResultStatus { get; set; }
    }
}


