using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RaceDayMVC.Helpers;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    [SessionAuthorize(RoleNames.Organiser)]
    public class OrganiserController : BaseController
    {
        private static readonly string[] TimeFormats =
            { @"h\:mm\:ss", @"hh\:mm\:ss", @"m\:ss", @"mm\:ss" };

        public OrganiserController(RaceDayApiService api) : base(api)
        {
        }

        // ---------- Dashboard ----------

        public async Task<IActionResult> Index()
        {
            var eventsTask = Api.GetMyEventsAsync();
            var enrolmentsTask = Api.GetEnrolmentsAsync();
            var typesTask = Api.GetEventTypesAsync();
            var routesTask = Api.GetRoutesAsync();

            await Task.WhenAll(eventsTask, enrolmentsTask, typesTask, routesTask);

            var events = await eventsTask;
            var enrolments = await enrolmentsTask;

            if (events.Unauthorized || enrolments.Unauthorized)
                return SessionExpired();

            var model = new OrganiserDashboardViewModel
            {
                OrganiserName = HttpContext.Session.GetString(SessionKeys.FullName) ?? ""
            };

            if (!events.Success || events.Data == null)
            {
                ViewData["Error"] = events.Error;
                return View(model);
            }

            model.Events = BuildRows(events.Data, enrolments.Data, (await typesTask).Data, (await routesTask).Data);
            model.TotalEvents = model.Events.Count;
            model.TotalEnrolments = model.Events.Sum(r => r.EnrolmentCount);
            model.UpcomingEvents = model.Events.Count(r => r.Event.EventDate.Date >= DateTime.Today);

            return View(model);
        }

        // ---------- Events management ----------

        public async Task<IActionResult> Events()
        {
            var eventsTask = Api.GetMyEventsAsync();
            var enrolmentsTask = Api.GetEnrolmentsAsync();
            var typesTask = Api.GetEventTypesAsync();
            var routesTask = Api.GetRoutesAsync();

            await Task.WhenAll(eventsTask, enrolmentsTask, typesTask, routesTask);

            var events = await eventsTask;
            var enrolments = await enrolmentsTask;

            if (events.Unauthorized || enrolments.Unauthorized)
                return SessionExpired();

            if (!events.Success || events.Data == null)
            {
                ViewData["Error"] = events.Error;
                return View(new List<EventSummaryRow>());
            }

            return View(BuildRows(events.Data, enrolments.Data, (await typesTask).Data, (await routesTask).Data));
        }

        private static List<EventSummaryRow> BuildRows(
            List<RaceEventDto> events,
            List<RegistrationDto>? enrolments,
            List<CategoryDto>? types,
            List<RouteDto>? routes)
        {
            var counts = (enrolments ?? new List<RegistrationDto>())
                .Where(r => !string.Equals(r.PaymentStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                .GroupBy(r => r.EventID)
                .ToDictionary(g => g.Key, g => g.Count());

            var typeLookup = (types ?? new List<CategoryDto>()).ToDictionary(t => t.CategoryID);
            var routeLookup = (routes ?? new List<RouteDto>()).ToDictionary(r => r.RouteId);

            return events.OrderBy(e => e.EventDate).Select(e => new EventSummaryRow
            {
                Event = e,
                EnrolmentCount = counts.TryGetValue(e.EventID, out int count) ? count : 0,
                TypeName = typeLookup.TryGetValue(e.CategoryID, out CategoryDto? type) ? type.CategoryName : "",
                DistanceKm = routeLookup.TryGetValue(e.RouteID, out RouteDto? route) ? route.DistanceKm : null
            }).ToList();
        }

        // Finds one of this organiser's events (organisers can only manage their own).
        private async Task<(RaceEventDto? Event, IActionResult? Redirect)> FindMyEventAsync(int id)
        {
            var result = await Api.GetMyEventsAsync();

            if (result.Unauthorized)
                return (null, SessionExpired());

            if (!result.Success || result.Data == null)
            {
                TempData["Error"] = result.Error;
                return (null, RedirectToAction("Events"));
            }

            var found = result.Data.FirstOrDefault(e => e.EventID == id);

            if (found == null)
            {
                TempData["Error"] = "That event could not be found among your events.";
                return (null, RedirectToAction("Events"));
            }

            return (found, null);
        }

        // ---------- Create / edit / delete ----------

        private async Task FillEventTypesAsync(EventFormViewModel model)
        {
            var types = await Api.GetEventTypesAsync();

            model.EventTypeItems = (types.Data ?? new List<CategoryDto>())
                .Select(t => new SelectListItem(t.CategoryName, t.CategoryID.ToString()))
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> CreateEvent()
        {
            var model = new EventFormViewModel { EventDate = DateTime.Today.AddDays(30) };
            await FillEventTypesAsync(model);
            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(6_000_000)]
        public async Task<IActionResult> CreateEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillEventTypesAsync(model);
                return View(model);
            }

            // Every event needs a route that holds its distance.
            var routeResult = await Api.CreateRouteAsync(new RouteDto
            {
                RouteName = $"{Truncate(model.EventName.Trim(), 80)} ({Guid.NewGuid().ToString("N").Substring(0, 6)})",
                DistanceKm = model.DistanceKm!.Value,
                StartPoint = model.Location.Trim(),
                FinishPoint = model.Location.Trim()
            });

            if (routeResult.Unauthorized)
                return SessionExpired();

            if (!routeResult.Success || routeResult.Data == null)
            {
                ModelState.AddModelError("", routeResult.Error);
                await FillEventTypesAsync(model);
                return View(model);
            }

            var eventResult = await Api.CreateEventAsync(ToDto(model, routeResult.Data.RouteId));

            if (eventResult.Unauthorized)
                return SessionExpired();

            if (!eventResult.Success || eventResult.Data == null)
            {
                ModelState.AddModelError("", eventResult.Error);
                await FillEventTypesAsync(model);
                return View(model);
            }

            string message = "Event created. Now add age or distance categories so participants can choose one.";

            if (model.Banner != null && model.Banner.Length > 0)
            {
                var upload = await Api.UploadEventBannerAsync(eventResult.Data.EventID, model.Banner);

                if (!upload.Success)
                    TempData["Error"] = "The event was created, but the banner could not be uploaded: " + upload.Error;
            }

            TempData["Success"] = message;

            return RedirectToAction("Categories", new { eventId = eventResult.Data.EventID });
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var (found, redirect) = await FindMyEventAsync(id);

            if (redirect != null || found == null)
                return redirect ?? RedirectToAction("Events");

            var route = await Api.GetRouteAsync(found.RouteID);

            var model = new EventFormViewModel
            {
                EventID = found.EventID,
                OrganizerID = found.OrganizerID,
                RouteID = found.RouteID,
                ExistingBannerUrl = found.BannerUrl,
                EventName = found.EventName,
                Description = found.Description,
                EventDate = found.EventDate.Date,
                Location = found.Location,
                DistanceKm = route.Data?.DistanceKm,
                CategoryID = found.CategoryID,
                EntryFee = found.EntryFee,
                Status = found.Status
            };

            await FillEventTypesAsync(model);

            return View(model);
        }

        [HttpPost]
        [RequestSizeLimit(6_000_000)]
        public async Task<IActionResult> EditEvent(EventFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await FillEventTypesAsync(model);
                return View(model);
            }

            // Keep the route's distance and places in step with the event.
            var routeResult = await Api.GetRouteAsync(model.RouteID);

            if (routeResult.Unauthorized)
                return SessionExpired();

            if (routeResult.Success && routeResult.Data != null)
            {
                var route = routeResult.Data;
                route.DistanceKm = model.DistanceKm!.Value;
                route.StartPoint = model.Location.Trim();
                route.FinishPoint = model.Location.Trim();

                var routeUpdate = await Api.UpdateRouteAsync(route);

                if (!routeUpdate.Success)
                {
                    ModelState.AddModelError("", routeUpdate.Error);
                    await FillEventTypesAsync(model);
                    return View(model);
                }
            }

            var eventResult = await Api.UpdateEventAsync(ToDto(model, model.RouteID));

            if (eventResult.Unauthorized)
                return SessionExpired();

            if (!eventResult.Success)
            {
                ModelState.AddModelError("", eventResult.Error);
                await FillEventTypesAsync(model);
                return View(model);
            }

            if (model.Banner != null && model.Banner.Length > 0)
            {
                var upload = await Api.UploadEventBannerAsync(model.EventID, model.Banner);

                if (!upload.Success)
                {
                    TempData["Error"] = "The event was saved, but the banner could not be uploaded: " + upload.Error;
                    return RedirectToAction("Events");
                }
            }

            TempData["Success"] = "Event updated.";

            return RedirectToAction("Events");
        }

        private static RaceEventDto ToDto(EventFormViewModel model, int routeId)
        {
            return new RaceEventDto
            {
                EventID = model.EventID,
                OrganizerID = model.OrganizerID,
                CategoryID = model.CategoryID!.Value,
                RouteID = routeId,
                EventName = model.EventName.Trim(),
                Description = model.Description?.Trim(),
                EventDate = model.EventDate!.Value.Date,
                Location = model.Location.Trim(),
                EntryFee = model.EntryFee,
                Status = string.IsNullOrWhiteSpace(model.Status) ? "Open" : model.Status
            };
        }

        private static string Truncate(string text, int max)
        {
            return text.Length <= max ? text : text.Substring(0, max);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await Api.DeleteEventAsync(id);

            if (result.Unauthorized)
                return SessionExpired();

            if (result.Success)
                TempData["Success"] = "Event deleted.";
            else
                TempData["Error"] = result.Error;

            return RedirectToAction("Events");
        }

        // ---------- Age / distance categories ----------

        [HttpGet]
        public async Task<IActionResult> Categories(int eventId)
        {
            var (found, redirect) = await FindMyEventAsync(eventId);

            if (redirect != null || found == null)
                return redirect ?? RedirectToAction("Events");

            var categories = await Api.GetEventCategoriesAsync(eventId);

            if (categories.Unauthorized)
                return SessionExpired();

            return View(new EventCategoriesViewModel
            {
                Event = found,
                Categories = categories.Data ?? new List<EventCategoryDto>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryFormModel form)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "Please check the category details.";

                return RedirectToAction("Categories", new { eventId = form.EventID });
            }

            var result = await Api.CreateEventCategoryAsync(new EventCategoryDto
            {
                EventID = form.EventID,
                Name = form.Name.Trim(),
                CategoryType = form.CategoryType,
                Description = form.Description
            });

            if (result.Unauthorized)
                return SessionExpired();

            if (result.Success)
                TempData["Success"] = "Category added.";
            else
                TempData["Error"] = result.Error;

            return RedirectToAction("Categories", new { eventId = form.EventID });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int categoryId, int eventId)
        {
            var result = await Api.DeleteEventCategoryAsync(categoryId);

            if (result.Unauthorized)
                return SessionExpired();

            if (result.Success)
                TempData["Success"] = "Category removed.";
            else
                TempData["Error"] = result.Error;

            return RedirectToAction("Categories", new { eventId });
        }

        // ---------- Enrolments ----------

        [HttpGet]
        public async Task<IActionResult> Enrolments(int eventId)
        {
            var (found, redirect) = await FindMyEventAsync(eventId);

            if (redirect != null || found == null)
                return redirect ?? RedirectToAction("Events");

            var result = await Api.GetEventEnrolmentsAsync(eventId);

            if (result.Unauthorized)
                return SessionExpired();

            if (!result.Success)
                ViewData["Error"] = result.Error;

            return View(new EventEnrolmentsViewModel
            {
                Event = found,
                Enrolments = result.Data ?? new List<EnrolmentDetailDto>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int registrationId, int eventId, string status)
        {
            var result = await Api.UpdateEnrolmentStatusAsync(registrationId, status);

            if (result.Unauthorized)
                return SessionExpired();

            if (result.Success)
                TempData["Success"] = "Enrolment marked as " + status + ".";
            else
                TempData["Error"] = result.Error;

            return RedirectToAction("Enrolments", new { eventId });
        }

        // ---------- Results entry ----------

        [HttpGet]
        public async Task<IActionResult> Results(int eventId)
        {
            var (found, redirect) = await FindMyEventAsync(eventId);

            if (redirect != null || found == null)
                return redirect ?? RedirectToAction("Events");

            if (found.EventDate.Date > DateTime.Today)
            {
                TempData["Error"] = "Results can only be entered once the event has taken place.";
                return RedirectToAction("Events");
            }

            var model = await BuildResultsModelAsync(found);

            if (model == null)
                return RedirectToAction("Events");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Results(int eventId, List<ResultRowInput> rows)
        {
            var (found, redirect) = await FindMyEventAsync(eventId);

            if (redirect != null || found == null)
                return redirect ?? RedirectToAction("Events");

            if (found.EventDate.Date > DateTime.Today)
            {
                TempData["Error"] = "Results can only be entered once the event has taken place.";
                return RedirectToAction("Events");
            }

            rows ??= new List<ResultRowInput>();

            var route = await Api.GetRouteAsync(found.RouteID);
            decimal distance = route.Data?.DistanceKm ?? 0;

            // Check every row first, so nothing is half-saved.
            var errors = new List<string>();
            var parsed = new Dictionary<int, TimeSpan>();

            foreach (var row in rows)
            {
                bool finished = row.ResultStatus == "Completed";

                // Rows that did not finish, or have no time typed yet, are not validated.
                if (!finished || string.IsNullOrWhiteSpace(row.FinishTime))
                    continue;

                if (!TimeSpan.TryParseExact(row.FinishTime.Trim(), TimeFormats,
                        CultureInfo.InvariantCulture, out TimeSpan time) || time <= TimeSpan.Zero)
                {
                    errors.Add($"{row.ParticipantName}: enter the finish time like 01:45:30.");
                    continue;
                }

                if (row.Position.HasValue && row.Position.Value < 1)
                {
                    errors.Add($"{row.ParticipantName}: position must be 1 or higher.");
                    continue;
                }

                parsed[row.RegistrationID] = time;
            }

            if (errors.Count > 0)
            {
                TempData["Error"] = string.Join(" ", errors);

                return View(new ResultsEntryViewModel
                {
                    EventID = found.EventID,
                    EventName = found.EventName,
                    EventDate = found.EventDate,
                    DistanceKm = distance > 0 ? distance : null,
                    Rows = rows
                });
            }

            // Anyone without a typed position is ranked by finish time.
            var ranking = parsed.OrderBy(p => p.Value)
                .Select((p, index) => new { p.Key, Rank = index + 1 })
                .ToDictionary(x => x.Key, x => x.Rank);

            int saved = 0;
            var failures = new List<string>();

            foreach (var row in rows)
            {
                string? finishTime = null;
                int? position = null;
                decimal? pace = null;

                if (row.ResultStatus == "Completed")
                {
                    if (!parsed.TryGetValue(row.RegistrationID, out TimeSpan time))
                        continue;

                    finishTime = time.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    position = row.Position ?? ranking[row.RegistrationID];

                    if (distance > 0)
                        pace = Math.Round((decimal)time.TotalMinutes / distance, 2);
                }

                var result = await Api.SaveResultAsync(
                    row.RegistrationID, finishTime, position, pace, row.ResultStatus);

                if (result.Unauthorized)
                    return SessionExpired();

                if (result.Success)
                    saved++;
                else
                    failures.Add($"{row.ParticipantName}: {result.Error}");
            }

            if (failures.Count > 0)
                TempData["Error"] = "Some results could not be saved. " + string.Join(" ", failures);

            if (saved > 0)
                TempData["Success"] = $"{saved} result(s) published.";

            return RedirectToAction("Results", new { eventId });
        }

        private async Task<ResultsEntryViewModel?> BuildResultsModelAsync(RaceEventDto found)
        {
            var enrolments = await Api.GetEventEnrolmentsAsync(found.EventID);
            var route = await Api.GetRouteAsync(found.RouteID);

            if (!enrolments.Success || enrolments.Data == null)
            {
                TempData["Error"] = enrolments.Error;
                return null;
            }

            return new ResultsEntryViewModel
            {
                EventID = found.EventID,
                EventName = found.EventName,
                EventDate = found.EventDate,
                DistanceKm = route.Data?.DistanceKm,
                Rows = enrolments.Data
                    .Where(e => !string.Equals(e.PaymentStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                    .Select(e => new ResultRowInput
                    {
                        RegistrationID = e.RegistrationID,
                        ParticipantName = e.ParticipantName,
                        CategoryName = e.CategoryName ?? "",
                        FinishTime = e.FinishTime,
                        Position = e.Position,
                        ResultStatus = string.IsNullOrWhiteSpace(e.ResultStatus) || e.ResultStatus == "Registered"
                            ? "Completed"
                            : e.ResultStatus
                    })
                    .ToList()
            };
        }
    }
}
