using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Helpers;
using RaceDayMVC.Models;
using RaceDayMVC.Services;

namespace RaceDayMVC.Controllers
{
    public class AccountController : BaseController
    {
        public AccountController(RaceDayApiService api) : base(api)
        {
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.IsLoggedIn() && !HttpContext.Session.IsTokenExpired())
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await Api.LoginAsync(model.Email.Trim(), model.Password);

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError("", result.Error);
                return View(model);
            }

            string role = RoleNames.Normalise(result.Data.Role);

            if (string.IsNullOrEmpty(role) || string.IsNullOrWhiteSpace(result.Data.Token))
            {
                ModelState.AddModelError("", "Your account could not be signed in. Please contact support.");
                return View(model);
            }

            // Keep the token, name, email and role in the session for every page.
            HttpContext.Session.SignIn(result.Data, role);

            return role == RoleNames.Organiser
                ? RedirectToAction("Index", "Organiser")
                : RedirectToAction("Index", "Participant");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.IsLoggedIn() && !HttpContext.Session.IsTokenExpired())
                return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await Api.RegisterAsync(
                model.Name.Trim(),
                model.Email.Trim(),
                model.Password,
                string.IsNullOrWhiteSpace(model.OrganiserCode) ? null : model.OrganiserCode.Trim());

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Error);
                return View(model);
            }

            TempData["Success"] = "Your account has been created. Please log in.";

            return RedirectToAction("Login");
        }

        // POST: Account/Logout - ends the session and returns to the home page.
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            TempData["Success"] = "You have been logged out.";

            return RedirectToAction("Index", "Home");
        }
    }
}
