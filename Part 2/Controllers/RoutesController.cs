using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using Part_2.Models;

namespace Part_2.Controllers
{//Start of namespace

    // Defines the Routes API.
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {// Start of class

        // Provides access to route records.
        private readonly RaceDayContext _context;

        public RoutesController(RaceDayContext context)
        {// Start of constructor

            // Assigns the database context to a private field.
            _context = context;

        }// End of constructor

        // GET: api/Routes
        // Returns all race routes.
        [HttpGet]
        public async Task<IActionResult> GetRoutes()
        {// Start of GetRoutes method

            // Retrieves all race routes from the database.
            var routes = await _context.RaceRoutes.ToListAsync();

            // Returns the list of race routes.
            return Ok(routes);

        }// End of GetRoutes method

        // GET: api/Routes/1
        // Returns one route by its ID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoute(int id)
        {// Start of GetRoute method

            // Finds the race route with the specified ID.
            var route = await _context.RaceRoutes.FindAsync(id);

            // If the route is not found, return a 404 Not Found response.
            if (route == null)
                return NotFound("Route not found.");

            // Returns the found race route.
            return Ok(route);
        }// End of GetRoute method

        // POST: api/Routes
        // Allows organisers and administrators to create routes.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateRoute(
            RaceRoute raceroute)
        {// Start of CreateRoute method

            // Adds the new race route to the database context.
            _context.RaceRoutes.Add(raceroute);
            // Saves the changes to the database asynchronously.
            await _context.SaveChangesAsync();
            // Returns a 201 Created response with the location of the new route.
            return CreatedAtAction(
                nameof(GetRoute),
                new { id = raceroute.RouteId },
                raceroute);
        }// End of CreateRoute method

        // PUT: api/Routes/1
        // Updates an existing race route.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoute(
            int id, RaceRoute updatedRoute)
        {// Start of UpdateRoute method

            // Checks if the provided ID matches the ID of the updated route.
            if (id != updatedRoute.RouteId)

                // If the IDs do not match, return a 400 Bad Request response.
                return BadRequest("Route ID does not match.");

            // Finds the existing race route in the database by its ID.
            var route = await _context.RaceRoutes.FindAsync(id);

            // If the route is not found, return a 404 Not Found response.
            if (route == null)
                return NotFound("Route not found.");
            
            // Updates the existing race route with the new values. 
            _context.Entry(route)
                .CurrentValues.SetValues(updatedRoute);

            // Saves the changes to the database asynchronously.
            await _context.SaveChangesAsync();

            // Returns a 200 OK response with the updated route.
            return Ok(route);
        }// End of UpdateRoute method

        // DELETE: api/Routes/1
        // Only administrators can delete routes.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {// Start of DeleteRoute method

            // Finds the race route with the specified ID in the database.
            var route = await _context.RaceRoutes.FindAsync(id);

            // If the route is not found, return a 404 Not Found response.
            if (route == null)
                return NotFound("Route not found.");
            
            // Removes the race route from the database context.
            _context.RaceRoutes.Remove(route);

            // Saves the changes to the database asynchronously.
            await _context.SaveChangesAsync();

            // Returns a 204 No Content response indicating successful deletion.
            return NoContent();

        }// End of DeleteRoute method
    }// End of class
}// End of namespace
    
