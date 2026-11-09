using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Helpers;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    [SessionAuthorize(RoleNames.Participant)]
    public class ParticipantController : BaseController
    {
        private static readonly string[] ClosedStatuses = { "Closed", "Cancelled", "Completed" };

        public ParticipantController(RaceDayApiService api) : base(api)
        {
        }

        private static bool IsOpenForEnrolment(RaceEventDto e)
        {
            return e.EventDate.Date >= DateTime.Today &&
                   !ClosedStatuses.Contains(e.Status, StringComparer.OrdinalIgnoreCase);
        }

        // Home page: all upcoming events, with filters.
        public async Task<IActionResult> Index(string? search, int? typeId, string? distance, string? period)
        {
            var model = new ParticipantHomeViewModel
            {
                Search = search,
                TypeId = typeId,
                Distance = distance,
                Period = period
            };

            var eventsTask = Api.GetEventsAsync();
            var typesTask = Api.GetEventTypesAsync();
            var routesTask = Api.GetRoutesAsync();
            var enrolmentsTask = Api.GetEnrolmentsAsync();

            await Task.WhenAll(eventsTask, typesTask, routesTask, enrolmentsTask);

            var events = await eventsTask;
            var types = await typesTask;
            var routes = await routesTask;
            var enrolments = await enrolmentsTask;

            if (events.Unauthorized || enrolments.Unauthorized)
                return SessionExpired();

            if (!events.Success || events.Data == null)
            {
                model.LoadError = events.Error;
                return View(model);
            }

            model.EventTypes = types.Data ?? new List<CategoryDto>();

            var routeLookup = (routes.Data ?? new List<RouteDto>()).ToDictionary(r => r.RouteId);
            var typeLookup = model.EventTypes.ToDictionary(t => t.CategoryID);
            var enrolledIds = (enrolments.Data ?? new List<RegistrationDto>())
                .Select(r => r.EventID).ToHashSet();

            IEnumerable<RaceEventDto> query = events.Data.Where(IsOpenForEnrolment);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();

                query = query.Where(e =>
                    e.EventName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (e.Description ?? "").Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (typeId.HasValue)
                query = query.Where(e => e.CategoryID == typeId.Value);

            DateTime today = DateTime.Today;

            switch (period)
            {
                case "week": query = query.Where(e => e.EventDate.Date <= today.AddDays(7)); break;
                case "month": query = query.Where(e => e.EventDate.Date <= today.AddMonths(1)); break;
                case "3months": query = query.Where(e => e.EventDate.Date <= today.AddMonths(3)); break;
            }

            foreach (var e in query.OrderBy(e => e.EventDate))
            {
                decimal? km = routeLookup.TryGetValue(e.RouteID, out RouteDto? route) ? route.DistanceKm : null;

                if (!MatchesDistance(km, distance))
                    continue;

                model.Events.Add(new EventCardViewModel
                {
                    Event = e,
                    DistanceKm = km,
                    TypeName = typeLookup.TryGetValue(e.CategoryID, out CategoryDto? type) ? type.CategoryName : "",
                    IsEnrolled = enrolledIds.Contains(e.EventID)
                });
            }

            return View(model);
        }

        private static bool MatchesDistance(decimal? km, string? band)
        {
            if (string.IsNullOrWhiteSpace(band))
                return true;

            if (km == null)
                return false;

            switch (band)
            {
                case "lt5": return km < 5;
                case "5to10": return km >= 5 && km <= 10;
                case "10to21": return km > 10 && km <= 21.1m;
                case "over21": return km > 21.1m;
                default: return true;
            }
        }

        // Full details of one event, with its categories and the enrol form.
        public async Task<IActionResult> Details(int id)
        {
            var eventResult = await Api.GetEventAsync(id);

            if (eventResult.Unauthorized)
                return SessionExpired();

            if (!eventResult.Success || eventResult.Data == null)
            {
                TempData["Error"] = eventResult.Error;
                return RedirectToAction("Index");
            }

            var raceEvent = eventResult.Data;

            var typesTask = Api.GetEventTypesAsync();
            var routeTask = Api.GetRouteAsync(raceEvent.RouteID);
            var categoriesTask = Api.GetEventCategoriesAsync(id);
            var enrolmentsTask = Api.GetEnrolmentsAsync();

            await Task.WhenAll(typesTask, routeTask, categoriesTask, enrolmentsTask);

            var enrolments = await enrolmentsTask;

            if (enrolments.Unauthorized)
                return SessionExpired();

            var categories = (await categoriesTask).Data ?? new List<EventCategoryDto>();
            var mine = (enrolments.Data ?? new List<RegistrationDto>()).FirstOrDefault(r => r.EventID == id);

            var model = new EventDetailViewModel
            {
                Event = raceEvent,
                Route = (await routeTask).Data,
                Categories = categories,
                MyEnrolment = mine,
                CanEnrol = mine == null && IsOpenForEnrolment(raceEvent),
                TypeName = ((await typesTask).Data ?? new List<CategoryDto>())
                    .FirstOrDefault(t => t.CategoryID == raceEvent.CategoryID)?.CategoryName ?? ""
            };

            if (mine?.EventCategoryID != null)
                model.MyCategoryName = categories.FirstOrDefault(c => c.EventCategoryID == mine.EventCategoryID)?.Name;

            return View(model);
        }

        // Records the enrolment straight away.
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int? eventCategoryId)
        {
            var result = await Api.EnrolAsync(eventId, eventCategoryId);

            if (result.Unauthorized)
                return SessionExpired();

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("Details", new { id = eventId });
            }

            TempData["Success"] = "You are enrolled! Your enrolment is pending until the organiser confirms it.";

            return RedirectToAction("MyEnrolments");
        }

        // Personal view of everything the participant has enrolled for.
        public async Task<IActionResult> MyEnrolments()
        {
            var enrolmentsTask = Api.GetEnrolmentsAsync();
            var eventsTask = Api.GetEventsAsync();
            var categoriesTask = Api.GetAllEventCategoriesAsync();

            await Task.WhenAll(enrolmentsTask, eventsTask, categoriesTask);

            var enrolments = await enrolmentsTask;
            var events = await eventsTask;

            if (enrolments.Unauthorized || events.Unauthorized)
                return SessionExpired();

            if (!enrolments.Success || !events.Success)
            {
                TempData["Error"] = !enrolments.Success ? enrolments.Error : events.Error;
                return View(new List<EnrolmentRowViewModel>());
            }

            var eventLookup = events.Data!.ToDictionary(e => e.EventID);
            var categoryLookup = (await categoriesTask).Data?.ToDictionary(c => c.EventCategoryID)
                ?? new Dictionary<int, EventCategoryDto>();
            var rows = new List<EnrolmentRowViewModel>();

            foreach (var r in enrolments.Data!)
            {
                eventLookup.TryGetValue(r.EventID, out RaceEventDto? e);

                string categoryName = "";

                if (r.EventCategoryID.HasValue &&
                    categoryLookup.TryGetValue(r.EventCategoryID.Value, out EventCategoryDto? category))
                {
                    categoryName = category.Name;
                }

                rows.Add(new EnrolmentRowViewModel
                {
                    Registration = r,
                    Event = e,
                    CategoryName = categoryName,
                    CanCancel = e != null && e.EventDate.Date >= DateTime.Today && r.FinishTime == null
                });
            }

            return View(rows.OrderBy(x => x.Event?.EventDate ?? DateTime.MaxValue).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> CancelEnrolment(int registrationId)
        {
            var result = await Api.CancelEnrolmentAsync(registrationId);

            if (result.Unauthorized)
                return SessionExpired();

            if (result.Success)
                TempData["Success"] = "Your enrolment has been cancelled.";
            else
                TempData["Error"] = result.Error;

            return RedirectToAction("MyEnrolments");
        }

        // Personal race history: finish time and position for completed events.
        public async Task<IActionResult> MyResults()
        {
            var enrolmentsTask = Api.GetEnrolmentsAsync();
            var resultsTask = Api.GetResultsAsync();
            var eventsTask = Api.GetEventsAsync();
            var categoriesTask = Api.GetAllEventCategoriesAsync();

            await Task.WhenAll(enrolmentsTask, resultsTask, eventsTask, categoriesTask);

            var enrolments = await enrolmentsTask;
            var results = await resultsTask;
            var events = await eventsTask;

            if (enrolments.Unauthorized || results.Unauthorized || events.Unauthorized)
                return SessionExpired();

            if (!enrolments.Success || !results.Success || !events.Success)
            {
                TempData["Error"] = !enrolments.Success ? enrolments.Error
                    : !results.Success ? results.Error : events.Error;
                return View(new List<ResultRowViewModel>());
            }

            var eventLookup = events.Data!.ToDictionary(e => e.EventID);
            var categoryLookup = (await categoriesTask).Data?.ToDictionary(c => c.EventCategoryID)
                ?? new Dictionary<int, EventCategoryDto>();

            // Everyone's finished results, used to count finishers per event.
            var finishersPerEvent = results.Data!
                .Where(r => string.Equals(r.ResultStatus, "Completed", StringComparison.OrdinalIgnoreCase) ||
                            string.IsNullOrEmpty(r.ResultStatus))
                .GroupBy(r => r.EventID)
                .ToDictionary(g => g.Key, g => g.Count());

            var rows = new List<ResultRowViewModel>();

            foreach (var r in enrolments.Data!.Where(x => !string.IsNullOrWhiteSpace(x.FinishTime)))
            {
                if (!eventLookup.TryGetValue(r.EventID, out RaceEventDto? e))
                    continue;

                string categoryName = "";

                if (r.EventCategoryID.HasValue &&
                    categoryLookup.TryGetValue(r.EventCategoryID.Value, out EventCategoryDto? category))
                {
                    categoryName = category.Name;
                }

                rows.Add(new ResultRowViewModel
                {
                    EventName = e.EventName,
                    EventDate = e.EventDate,
                    CategoryName = categoryName,
                    FinishTime = r.FinishTime ?? "",
                    Position = r.Position,
                    ResultStatus = r.ResultStatus ?? "Completed",
                    TotalFinishers = finishersPerEvent.TryGetValue(r.EventID, out int total) ? total : 0
                });
            }

            return View(rows.OrderByDescending(x => x.EventDate).ToList());
        }

        // Profile page.
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await Api.GetProfileAsync();

            if (result.Unauthorized)
                return SessionExpired();

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("Index");
            }

            return View(new ProfileViewModel
            {
                FullName = result.Data.FullName,
                Email = result.Data.Email,
                ProfilePictureUrl = result.Data.ProfilePictureUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            // The picture is not part of this form; keep whatever is stored.
            model.ProfilePictureUrl = HttpContext.Session.GetString(SessionKeys.Picture);

            if (!ModelState.IsValid)
                return View(model);

            var result = await Api.UpdateProfileAsync(model.FullName.Trim(), model.Email.Trim());

            if (result.Unauthorized)
                return SessionExpired();

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error);
                return View(model);
            }

            HttpContext.Session.SetString(SessionKeys.FullName, model.FullName.Trim());
            HttpContext.Session.SetString(SessionKeys.Email, model.Email.Trim().ToLowerInvariant());

            TempData["Success"] = "Your details have been updated.";

            return RedirectToAction("Profile");
        }

        // Sends the chosen picture to the API, which stores it in Azure Blob Storage.
        [HttpPost]
        [RequestSizeLimit(6_000_000)]
        public async Task<IActionResult> UploadPicture(IFormFile? picture)
        {
            if (picture == null || picture.Length == 0)
            {
                TempData["Error"] = "Please choose an image to upload.";
                return RedirectToAction("Profile");
            }

            var result = await Api.UploadProfilePictureAsync(picture);

            if (result.Unauthorized)
                return SessionExpired();

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Error;
                return RedirectToAction("Profile");
            }

            HttpContext.Session.SetString(SessionKeys.Picture, result.Data.Url);

            TempData["Success"] = "Your profile picture has been updated.";

            return RedirectToAction("Profile");
        }
    }
}
