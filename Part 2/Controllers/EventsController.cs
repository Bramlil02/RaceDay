using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using Part_2.Models;

namespace Part_2.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : Controller
    {

        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: /api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RaceEvent>>> GetEvents()
        {
            var events = await _context.RaceEvents.ToListAsync();
            return Ok(events);
        }

        // GET: /api/events/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RaceEvent>> GetEvent(int id)
        {
            var raceEvent = await _context.RaceEvents
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (raceEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(raceEvent);
        }
    }

}

