using Microsoft.AspNetCore.Mvc;

namespace RaceDayMVC.Controllers
{
    public class OrganiserController : Controller
    {
        public IActionResult Index()
        {
            string? token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            string? role = HttpContext.Session.GetString("Role");

            if (role != "Organiser" && role != "Organizer")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}