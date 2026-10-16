using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;

namespace Part_2.Controllers
{//Start of namespace

    // Defines the Results API and requires login for all endpoints.
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {// Start of class

        // Provides access to race registration records.
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {// Start of constructor

            // Assigns the database context to a private field.
            _context = context;

        }// End of constructor

        // GET: api/Results
        // Returns registrations that have a recorded finish time.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {// Start of GetResults method
            // Retrieves all registrations with a non-null finish time.
            var results = await _context.Registrations
                .Where(r => r.FinishTime != null)
                .ToListAsync();
            
            // Returns the list of results.
            return Ok(results);

        }// End of GetResults method

        // GET: api/Results/1
        // The ID here is the RegistrationID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {// Start of GetResult method

            // Retrieves a single registration by its RegistrationID.
            var result = await _context.Registrations
                .FirstOrDefaultAsync(r => r.RegistrationID == id);

            // If no registration is found, return a 404 Not Found response.
            if (result == null)
                return NotFound("Result not found.");

            // Returns the found registration.
            return Ok(result);

        }// End of GetResult method

        // GET: api/Results/1
        // Retrieves one result using its RegistrationID.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> RecordResult(
            RecordResultRequest request)
        {// Start of RecordResult method
            // Validates the request and retrieves the registration record.
            var registration = await _context.Registrations
                .FindAsync(request.RegistrationID);

            // If no registration is found, return a 404 Not Found response.
            if (registration == null)
                return NotFound("Registration not found.");

            // Validates that the FinishTime is provided in the request.
            if (string.IsNullOrWhiteSpace(request.FinishTime))
                // If FinishTime is not provided, return a 400 Bad Request response.
                return BadRequest("Finish time is required.");

            // Updates the registration record with the provided result details.
            registration.FinishTime = request.FinishTime;
            registration.Position = request.Position;
            registration.AveragePace = request.AveragePace;
            registration.ResultStatus = request.ResultStatus;

            // Saves the changes to the database asynchronously.
            await _context.SaveChangesAsync();
            // Returns a success response with the updated registration record.
            return Ok(new
            {// Start of anonymous object

                // Returns a success message and the updated registration record.
                message = "Result recorded successfully.",
                registration

            });// End of anonymous object
        }// End of RecordResult method

        // PUT: api/Results/1
        // Updates an existing result using its RegistrationID.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResult(
            int id, RecordResultRequest request)
        {// Start of UpdateResult method

            // Validates that the RegistrationID in the request matches the ID in the URL.
            if (id != request.RegistrationID)
                // If they do not match, return a 400 Bad Request response.
                return BadRequest("Registration ID does not match.");

            // Retrieves the registration record by its RegistrationID.
            var registration = await _context.Registrations
                .FindAsync(id);
            
            // If no registration is found, return a 404 Not Found response.
            if (registration == null)
                return NotFound("Registration not found.");

            // Validates that the FinishTime is provided in the request.
            registration.FinishTime = request.FinishTime;
            registration.Position = request.Position;
            registration.AveragePace = request.AveragePace;
            registration.ResultStatus = request.ResultStatus;

            // Saves the changes to the database asynchronously.
            await _context.SaveChangesAsync();
            
            // Returns a success response with the updated registration record.
            return Ok(new
            {// Start of anonymous object

                // Returns a success message and the updated registration record.
                message = "Result updated successfully.",
                registration

            });// End of anonymous object
        }// End of UpdateResult method
    }

    public class RecordResultRequest
    {// Start of RecordResultRequest class

        // The ID of the registration for which the result is being recorded or updated.
        public int RegistrationID { get; set; }

        // The finish time of the participant in the race.
        public string? FinishTime { get; set; }

        // The position of the participant in the race.
        public int? Position { get; set; }

        // The average pace of the participant during the race.
        public decimal? AveragePace { get; set; }

        // The status of the result (e.g., "Completed", "Disqualified").
        public string? ResultStatus { get; set; }


    }// End of RecordResultRequest class
}// End of namespace


