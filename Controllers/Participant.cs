using Microsoft.AspNetCore.Mvc;

namespace RaceDayMVC.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            // Check whether the user is logged in.
            if (string.IsNullOrEmpty(
                HttpContext.Session.GetString("JWToken")))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check whether the user is a Participant.
            string role = HttpContext.Session.GetString("Role") ?? "";

            if (role != "Participant")
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
    }
}