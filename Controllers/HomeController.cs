using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDayMVC.Helpers;
using RaceDayMVC.Models;

namespace RaceDayMVC.Controllers
{
    public class HomeController : Controller
    {
        // Public landing page. Logged-in users go straight to their own home page.
        public IActionResult Index()
        {
            var session = HttpContext.Session;

            if (session.IsLoggedIn() && !session.IsTokenExpired())
            {
                string role = session.GetRole() ?? "";

                if (role == RoleNames.Organiser)
                    return RedirectToAction("Index", "Organiser");

                if (role == RoleNames.Participant)
                    return RedirectToAction("Index", "Participant");
            }

            return View();
        }

        // Friendly page for errors and missing pages.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };

            if (code == 404)
            {
                model.Title = "Page not found";
                model.Message = "We could not find the page you were looking for.";
            }
            else if (code == 403)
            {
                model.Title = "Access denied";
                model.Message = "You do not have access to that page.";
            }

            return View(model);
        }
    }
}
