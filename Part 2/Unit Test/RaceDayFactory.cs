using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Part_2.Data;
using Microsoft.EntityFrameworkCore.InMemory;
namespace Part_2.Unit_Test
{//Start of namespace
    public class RaceDayFactory : WebApplicationFactory<Program>
    {// Start of class 
        protected override void ConfigureWebHost(
                IWebHostBuilder builder)
        {// Start of ConfigureWebHost method

            // Configures the web host for testing.
            builder.ConfigureServices(static services =>
                {// Start of ConfigureServices method

                    // Removes the existing DbContextOptions for RaceDayContext to avoid conflicts.
                    services.RemoveAll<DbContextOptions<RaceDayContext>>();

                    // Adds a new DbContext for RaceDayContext using an in-memory database for testing purposes.
                    services.AddDbContext<RaceDayContext>(static options =>
                    {// Start of AddDbContext method

                        // Configures the DbContext to use an in-memory database named "RaceDayTests".
                        object value = options.UseInMemoryDatabase("RaceDayTests");

                    });// End of AddDbContext method

                });// End of ConfigureServices method

        }// End of ConfigureWebHost method

    }// End of class

}// End of namespace



