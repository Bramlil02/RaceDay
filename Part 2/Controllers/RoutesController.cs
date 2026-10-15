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
    public class RoutesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public RoutesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Routes
        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {
            var routes = await _context.RaceRoutes.ToListAsync();
            return Ok(routes);
        }

        // GET: api/Routes/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoute(int id)
        {
            var route = await _context.RaceRoutes.FindAsync(id);

            if (route == null)
                return NotFound("Route not found.");

            return Ok(route);
        }

        // POST: api/Routes
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateRoute(
            RaceRoute raceroute)
        {
            _context.RaceRoutes.Add(raceroute);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetRoute),
                new { id = raceroute.RouteId },
                raceroute);
        }

        // PUT: api/Routes/1
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(
            int id, RaceRoute updatedRoute)
        {
            if (id != updatedRoute.RouteId)
                return BadRequest("Route ID does not match.");

            var route = await _context.RaceRoutes.FindAsync(id);

            if (route == null)
                return NotFound("Route not found.");

            _context.Entry(route)
                .CurrentValues.SetValues(updatedRoute);

            await _context.SaveChangesAsync();

            return Ok(route);
        }

        // DELETE: api/Routes/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.RaceRoutes.FindAsync(id);

            if (route == null)
                return NotFound("Route not found.");

            _context.RaceRoutes.Remove(route);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
    
