using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Part_2.Data;
using Part_2.Models;

namespace Part_2.Controllers
{//Start of namespace

    // Defines the Categories API.
    [Route("api/[controller]")]
    [ApiController]

    public class CategoriesController : ControllerBase
    {// Start of class
     // Provides access to category records.
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {// Start of constructor

            // Assigns the database context to a private field.
            _context = context;

        }// End of constructor

        // GET: api/Categories
        // Returns all categories.
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {// Start of GetCategories method

            // Retrieves all categories from the database.
            var categories = await _context.Categories
                .ToListAsync();

            // Returns the list of categories.
            return Ok(categories);

        }// End of GetCategories method

        // GET: api/Categories/1
        // Returns a category using its ID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {// Start of GetCategory method

            // Finds the category with the specified ID.
            var category = await _context.Categories
                .FindAsync(id);

            // If the category is not found, return a 404 Not Found response.
            if (category == null)
                return NotFound("Category not found.");

            // Returns the found category.
            return Ok(category);


        }// End of GetCategory method

        // POST: api/Categories
        // Allows organisers and administrators to add categories.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            Category category)
        {// Start of CreateCategory method

            // Adds the new category to the database context and saves changes.
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Returns the location of the newly created category.
            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryID },
                category);

        }// End of CreateCategory method

        // PUT: api/Categories/1
        // Updates an existing category.
        [Authorize(Roles = "Admin,Organizer")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id, Category updatedCategory)
        {// Start of UpdateCategory method

            // Checks if the provided ID matches the ID of the updated category.
            if (id != updatedCategory.CategoryID)
                return BadRequest("Category ID does not match.");

            // Finds the existing category in the database.
            var category = await _context.Categories
                .FindAsync(id);

            // If the category is not found, return a 404 Not Found response.
            if (category == null)
                return NotFound("Category not found.");

            // Copies the new values into the existing category.
            _context.Entry(category)
                .CurrentValues.SetValues(updatedCategory);

            // Saves the changes to the database.
            await _context.SaveChangesAsync();

            // Returns the updated category.
            return Ok(category);

        }// End of UpdateCategory method


        // DELETE: api/Categories/1
        // Only administrators can delete categories.
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {// Start of DeleteCategory method

            // Finds the category with the specified ID.
            var category = await _context.Categories
                .FindAsync(id);

            // If the category is not found, return a 404 Not Found response.
            if (category == null)
                return NotFound("Category not found.");
            
            // Removes the category from the database context and saves changes.
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            // Returns a 204 No Content response indicating successful deletion.
            return NoContent();

        }// End of DeleteCategory method
    }// End of class
}// End of namespace