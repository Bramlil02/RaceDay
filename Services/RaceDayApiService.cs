using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDayMVC.Helpers;
using RaceDayMVC.Models;

namespace RaceDayMVC.Services
{
    // The only place in the MVC project that talks to the outside world.
    // Every call goes to the Part 2 API; failures come back as friendly messages.
    public class RaceDayApiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _contextAccessor;

        public RaceDayApiService(
            IHttpClientFactory factory,
            IHttpContextAccessor contextAccessor)
        {
            _factory = factory;
            _contextAccessor = contextAccessor;
        }

        // ---------- Authentication ----------

        public Task<ApiResult<LoginResponseDto>> LoginAsync(string email, string password)
        {
            return SendAsync<LoginResponseDto>(
                c => c.PostAsJsonAsync("api/Auth/login", new { Email = email, Password = password }, JsonOptions),
                isAuthCall: true);
        }

        public Task<ApiResult<bool>> RegisterAsync(string name, string email, string password, string? organiserCode)
        {
            return SendAsync<bool>(
                c => c.PostAsJsonAsync("api/Auth/Register",
                    new { Name = name, Email = email, Password = password, OrganiserCode = organiserCode }, JsonOptions),
                isAuthCall: true);
        }

        // ---------- Events ----------

        public Task<ApiResult<List<RaceEventDto>>> GetEventsAsync()
            => SendAsync<List<RaceEventDto>>(c => c.GetAsync("api/Events"));

        public Task<ApiResult<List<RaceEventDto>>> GetMyEventsAsync()
            => SendAsync<List<RaceEventDto>>(c => c.GetAsync("api/Events/mine"));

        public Task<ApiResult<RaceEventDto>> GetEventAsync(int id)
            => SendAsync<RaceEventDto>(c => c.GetAsync($"api/Events/{id}"));

        public Task<ApiResult<RaceEventDto>> CreateEventAsync(RaceEventDto raceEvent)
            => SendAsync<RaceEventDto>(c => c.PostAsJsonAsync("api/Events", raceEvent, JsonOptions));

        public Task<ApiResult<RaceEventDto>> UpdateEventAsync(RaceEventDto raceEvent)
            => SendAsync<RaceEventDto>(c => c.PutAsJsonAsync($"api/Events/{raceEvent.EventID}", raceEvent, JsonOptions));

        public Task<ApiResult<bool>> DeleteEventAsync(int id)
            => SendAsync<bool>(c => c.DeleteAsync($"api/Events/{id}"));

        // ---------- Event types and routes ----------

        public Task<ApiResult<List<CategoryDto>>> GetEventTypesAsync()
            => SendAsync<List<CategoryDto>>(c => c.GetAsync("api/Categories"));

        public Task<ApiResult<List<RouteDto>>> GetRoutesAsync()
            => SendAsync<List<RouteDto>>(c => c.GetAsync("api/Routes"));

        public Task<ApiResult<RouteDto>> GetRouteAsync(int id)
            => SendAsync<RouteDto>(c => c.GetAsync($"api/Routes/{id}"));

        public Task<ApiResult<RouteDto>> CreateRouteAsync(RouteDto route)
            => SendAsync<RouteDto>(c => c.PostAsJsonAsync("api/Routes", route, JsonOptions));

        public Task<ApiResult<RouteDto>> UpdateRouteAsync(RouteDto route)
            => SendAsync<RouteDto>(c => c.PutAsJsonAsync($"api/Routes/{route.RouteId}", route, JsonOptions));

        // ---------- Age / distance categories of an event ----------

        public Task<ApiResult<List<EventCategoryDto>>> GetEventCategoriesAsync(int eventId)
            => SendAsync<List<EventCategoryDto>>(c => c.GetAsync($"api/EventCategories?eventId={eventId}"));

        public Task<ApiResult<List<EventCategoryDto>>> GetAllEventCategoriesAsync()
            => SendAsync<List<EventCategoryDto>>(c => c.GetAsync("api/EventCategories"));

        public Task<ApiResult<EventCategoryDto>> CreateEventCategoryAsync(EventCategoryDto category)
            => SendAsync<EventCategoryDto>(c => c.PostAsJsonAsync("api/EventCategories", category, JsonOptions));

        public Task<ApiResult<bool>> DeleteEventCategoryAsync(int id)
            => SendAsync<bool>(c => c.DeleteAsync($"api/EventCategories/{id}"));

        // ---------- Enrolments ----------

        // Participants get their own; organisers get the enrolments for their events.
        public Task<ApiResult<List<RegistrationDto>>> GetEnrolmentsAsync()
            => SendAsync<List<RegistrationDto>>(c => c.GetAsync("api/Enrolments"));

        public Task<ApiResult<List<EnrolmentDetailDto>>> GetEventEnrolmentsAsync(int eventId)
            => SendAsync<List<EnrolmentDetailDto>>(c => c.GetAsync($"api/Enrolments/event/{eventId}"));

        public Task<ApiResult<bool>> EnrolAsync(int eventId, int? eventCategoryId)
            => SendAsync<bool>(c => c.PostAsJsonAsync("api/Enrolments",
                new { EventID = eventId, EventCategoryID = eventCategoryId }, JsonOptions));

        public Task<ApiResult<bool>> CancelEnrolmentAsync(int registrationId)
            => SendAsync<bool>(c => c.DeleteAsync($"api/Enrolments/{registrationId}"));

        public Task<ApiResult<bool>> UpdateEnrolmentStatusAsync(int registrationId, string status)
            => SendAsync<bool>(c => c.PutAsJsonAsync($"api/Enrolments/{registrationId}/status",
                new { PaymentStatus = status }, JsonOptions));

        // ---------- Results ----------

        public Task<ApiResult<List<RegistrationDto>>> GetResultsAsync()
            => SendAsync<List<RegistrationDto>>(c => c.GetAsync("api/Results"));

        public Task<ApiResult<bool>> SaveResultAsync(
            int registrationId, string? finishTime, int? position, decimal? averagePace, string resultStatus)
            => SendAsync<bool>(c => c.PutAsJsonAsync($"api/Results/{registrationId}",
                new
                {
                    RegistrationID = registrationId,
                    FinishTime = finishTime,
                    Position = position,
                    AveragePace = averagePace,
                    ResultStatus = resultStatus
                }, JsonOptions));

        // ---------- Profile and uploads ----------

        public Task<ApiResult<ProfileDto>> GetProfileAsync()
            => SendAsync<ProfileDto>(c => c.GetAsync("api/Profile"));

        public Task<ApiResult<bool>> UpdateProfileAsync(string fullName, string email)
            => SendAsync<bool>(c => c.PutAsJsonAsync("api/Profile",
                new { FullName = fullName, Email = email }, JsonOptions));

        public Task<ApiResult<UploadResultDto>> UploadEventBannerAsync(int eventId, IFormFile file)
            => SendAsync<UploadResultDto>(c => PostFileAsync(c, $"api/Uploads/event-banner/{eventId}", file));

        public Task<ApiResult<UploadResultDto>> UploadProfilePictureAsync(IFormFile file)
            => SendAsync<UploadResultDto>(c => PostFileAsync(c, "api/Uploads/profile-picture", file));

        // ---------- Plumbing ----------

        // Creates a client that sends the logged-in user's token.
        private HttpClient CreateClient()
        {
            var client = _factory.CreateClient("RaceDayAPI");

            string? token = _contextAccessor.HttpContext?.Session.GetString(SessionKeys.Token);

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private static async Task<HttpResponseMessage> PostFileAsync(HttpClient client, string url, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);

            string contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "file", Path.GetFileName(file.FileName));

            return await client.PostAsync(url, content);
        }

        // Runs a call and turns the response (or any failure) into an ApiResult.
        private async Task<ApiResult<T>> SendAsync<T>(
            Func<HttpClient, Task<HttpResponseMessage>> call, bool isAuthCall = false)
        {
            try
            {
                using var client = CreateClient();
                using var response = await call(client);

                if (response.IsSuccessStatusCode)
                {
                    if (typeof(T) == typeof(bool))
                        return ApiResult<T>.Ok((T)(object)true);

                    var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);

                    return data is null
                        ? ApiResult<T>.Fail("The server sent an empty response.")
                        : ApiResult<T>.Ok(data);
                }

                return await FailAsync<T>(response, isAuthCall);
            }
            catch (HttpRequestException)
            {
                return ApiResult<T>.Fail("The RaceDay service is not reachable right now. Please try again shortly.");
            }
            catch (TaskCanceledException)
            {
                return ApiResult<T>.Fail("The RaceDay service took too long to respond. Please try again.");
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                return ApiResult<T>.Fail("The RaceDay service sent a response that could not be read.");
            }
        }

        private static async Task<ApiResult<T>> FailAsync<T>(HttpResponseMessage response, bool isAuthCall)
        {
            string body = "";

            try
            {
                body = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                // Ignore - a generic message is used below.
            }

            int status = (int)response.StatusCode;
            string? message = ExtractMessage(body);

            if (status == 401)
            {
                if (isAuthCall)
                    return ApiResult<T>.Fail(message ?? "Invalid email or password.");

                return ApiResult<T>.Fail("Your session has expired. Please log in again.", unauthorized: true);
            }

            if (status == 403)
                return ApiResult<T>.Fail("You do not have permission to do that.");

            if (status == 404)
                return ApiResult<T>.Fail(message ?? "The requested item could not be found.");

            if (status >= 500 && message == null)
                return ApiResult<T>.Fail("The RaceDay service had a problem. Please try again shortly.");

            return ApiResult<T>.Fail(message ?? "Something went wrong. Please check your details and try again.");
        }

        // Pulls a short, readable message out of an API error body.
        private static string? ExtractMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;

            body = body.Trim();

            if (body.StartsWith("{"))
            {
                try
                {
                    using var document = JsonDocument.Parse(body);
                    JsonElement root = document.RootElement;

                    if (root.TryGetProperty("errors", out JsonElement errors) &&
                        errors.ValueKind == JsonValueKind.Object)
                    {
                        foreach (JsonProperty property in errors.EnumerateObject())
                        {
                            if (property.Value.ValueKind == JsonValueKind.Array &&
                                property.Value.GetArrayLength() > 0)
                            {
                                return property.Value[0].GetString();
                            }
                        }
                    }

                    if (root.TryGetProperty("message", out JsonElement message) &&
                        message.ValueKind == JsonValueKind.String)
                    {
                        return message.GetString();
                    }
                }
                catch (JsonException)
                {
                    // Fall through to the generic message.
                }

                return null;
            }

            if (body.StartsWith("<") || body.Length > 200)
                return null;

            return body.Trim('"');
        }
    }
}
