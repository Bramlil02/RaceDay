using System.Net;
using Xunit;

namespace Part_2.Unit_Test
{
    public class AuthControllerTests : IClassFixture<RaceDayFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(RaceDayFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidDetails_ReturnsSuccess()
        {
            var request = new
            {
                fullName = "Test Participant",
                email = $"test{Guid.NewGuid():N}@example.com",
                password = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/register", request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithoutFullName_ReturnsBadRequest()
        {
            var request = new
            {
                fullName = "",
                email = "missingname@example.com",
                password = "TestPassword123!"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/register", request);

            Assert.AreEqual(
                HttpStatusCode.BadRequest,
                response.StatusCode);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
        {
            var request = new
            {
                email = $"unknown{Guid.NewGuid():N}@example.com",
                password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/Auth/login", request);

            Assert.AreEqual(
                HttpStatusCode.Unauthorized,
                response.StatusCode);
        }
    }
    
    }

