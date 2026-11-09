using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    // Shared behaviour for the controllers that call the API.
    public abstract class BaseController : Controller
    {
        protected readonly RaceDayApiService Api;

        protected BaseController(RaceDayApiService api)
        {
            Api = api;
        }

        // Used when the API says the token is no longer valid.
        protected IActionResult SessionExpired()
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Account");
        }
    }
}
