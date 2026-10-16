using System.Net;
using Xunit;

namespace Part_2.Unit_Test
{
    public class AuthorizationTests : IClassFixture<RaceDayFactory>
    {
        private readonly HttpClient _client;

        public AuthorizationTests(RaceDayFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateEvent_WithoutLogin_ReturnsUnauthorized()
        {
            var eventData = new
            {
                eventID = 0,
                organizerID = 1,
                categoryID = 1,
                routeID = 1,
                eventName = "Test Race",
                eventDate = DateTime.Today.AddDays(30),
                location = "Test Location",
                entryFee = 100,
                status = "Open"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Events", eventData);

            Assert.AreEqual(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
  
    }

