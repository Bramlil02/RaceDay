using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using Part_2.Models;

namespace Part_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.RaceEvents.ToListAsync();
            return Ok(events);
        }

        // GET: api/Events/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var raceEvent = await _context.RaceEvents
                .FindAsync(id);

            if (raceEvent == null)
                return NotFound("Event not found.");

            return Ok(raceEvent);
        }

        // POST: api/Events
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            RaceEvent raceEvent)
        {
            _context.RaceEvents.Add(raceEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = raceEvent.EventID },
                raceEvent);
        }

        // PUT: api/Events/1
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(
            int id, RaceEvent updatedEvent)
        {
            if (id != updatedEvent.EventID)
                return BadRequest("Event ID does not match.");

            var existingEvent = await _context.RaceEvents
                .FindAsync(id);

            if (existingEvent == null)
                return NotFound("Event not found.");

            _context.Entry(existingEvent)
                .CurrentValues.SetValues(updatedEvent);

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/Events/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var raceEvent = await _context.RaceEvents
                .FindAsync(id);

            if (raceEvent == null)
                return NotFound("Event not found.");

            _context.RaceEvents.Remove(raceEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

