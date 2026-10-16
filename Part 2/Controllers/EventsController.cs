using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using Part_2.Models;

namespace Part_2.Controllers
{//Start of namespace

    // Defines the Events API.
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {// Start of class

        // Provides access to the event database records.
        private readonly RaceDayContext _context;

        // Receives the database context through dependency injection.
        public EventsController(RaceDayContext context)
        {// Start of constructor

            // Assigns the database context to a private field.
            _context = context;

        }// End of constructor

        // GET: api/Events
        // Returns all events.
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {// Start of GetEvents method

            // Retrieves all events from the database.
            var events = await _context.RaceEvents.ToListAsync();
            return Ok(events);
        }// End of GetEvents method

        // GET: api/Events/1
        // Returns one event using its ID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {// Start of GetEvent method

            // Retrieves the event with the specified ID from the database.
            var raceEvent = await _context.RaceEvents
                .FindAsync(id);

            // Returns 404 if the event does not exist.
            if (raceEvent == null)
                return NotFound("Event not found.");

            return Ok(raceEvent);
        }// End of GetEvent method


        // POST: api/Events
        // Allows organisers and administrators to create events.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            RaceEvent raceEvent)
        {// Start of CreateEvent method

            // Adds the new event to the database.
            _context.RaceEvents.Add(raceEvent);
            await _context.SaveChangesAsync();

            // Returns a 201 Created response with the new event's details.
            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);

        }// End of CreateEvent method

        // PUT: api/Events/1
        // Allows organisers and administrators to update events.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id, RaceEvent updatedEvent)
        {// Start of UpdateEvent method
         // Ensures the URL ID matches the event ID in the request.
            if (id != updatedEvent.EventID)
                return BadRequest("Event ID does not match.");

            // Finds the existing event.
            var existingEvent = await _context.RaceEvents
                .FindAsync(id);


            if (existingEvent == null)
                return NotFound("Event not found.");

            // Copies the submitted values onto the existing record.
            _context.Entry(existingEvent)
                .CurrentValues.SetValues(updatedEvent);

            // Saves the updated event.
            await _context.SaveChangesAsync();

            return Ok(existingEvent);

        }// End of UpdateEvent method
        
        // DELETE: api/Events/1
        // Only administrators can delete events.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {// Start of DeleteEvent method

            // Finds the event to delete.
            var raceEvent = await _context.RaceEvents
                .FindAsync(id);

            // Returns 404 if the event does not exist.
            if (raceEvent == null)
                return NotFound("Event not found.");

            // Removes the event from the database.
            _context.RaceEvents.Remove(raceEvent);
            await _context.SaveChangesAsync();

            // Returns 204 No Content after successful deletion.
            return NoContent();
        }// End of DeleteEvent method
    }// End of class

}// End of namespace

