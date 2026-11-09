
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDayMVC.Models;

namespace RaceDayMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync(
                "api/Auth/login",
                new
                {
                    Email = model.Email,
                    Password = model.Password
                });

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    "",
                    "Login failed. Please check your email and password.");

                return View(model);
            }

            var responseText = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseText);

            var data = document.RootElement;

            string token = data.GetProperty("token").GetString() ?? "";
            string fullName = data.GetProperty("fullName").GetString() ?? "";
            string email = data.GetProperty("email").GetString() ?? "";
            string role = data.GetProperty("role").GetString() ?? "";

            // Display the role received from the API
            Console.WriteLine("========== LOGIN DEBUG ==========");
            Console.WriteLine("API ROLE: " + role);

            if (string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError(
                    "",
                    "The API did not return a login token.");

                return View(model);
            }

            // Clean the role returned by the API
            role = role.Trim();

            // Save the user's details in the session
            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("FullName", fullName);
            HttpContext.Session.SetString("Email", email);
            HttpContext.Session.SetString("Role", role);

            // Display the role saved in the session
            Console.WriteLine("SESSION ROLE: " +
                HttpContext.Session.GetString("Role"));

            // Redirect users according to their role
            if (role.Equals("Organiser",
                StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Organizer",
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("REDIRECTING TO ORGANISER DASHBOARD");

                return RedirectToAction("Index", "Organiser");
            }

            if (role.Equals("Participant",
                StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("PARTICIPANT REDIRECT SELECTED");

                return RedirectToAction("Index", "Participant");
            }

            Console.WriteLine("UNKNOWN ROLE - REDIRECTING TO HOME");

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                var response = await client.PostAsJsonAsync(
                    "api/Auth/Register",
                    new
                    {
                        Name = model.Name,
                        Email = model.Email,
                        Password = model.Password
                    });

                string responseMessage =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        "",
                        "Registration failed: " + responseMessage);

                    return View(model);
                }

                TempData["Success"] =
                    "Your account has been created. Please log in.";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    "Could not connect to the RaceDay API: " + ex.Message);

                return View(model);
            }
        }

        // GET: Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}
