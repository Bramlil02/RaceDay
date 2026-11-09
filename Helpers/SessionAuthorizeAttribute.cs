using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace RaceDayMVC.Helpers
{
    // Protects a controller: the user must be logged in (session) and have one of the roles.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public SessionAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;
            var session = http.Session;

            var tempData = http.RequestServices
                .GetRequiredService<ITempDataDictionaryFactory>()
                .GetTempData(http);

            // Not logged in, or the API token has run out.
            if (!session.IsLoggedIn() || session.IsTokenExpired())
            {
                bool expired = session.IsLoggedIn();

                session.Clear();

                tempData["Error"] = expired
                    ? "Your session has expired. Please log in again."
                    : "Please log in to continue.";

                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Logged in, but this page belongs to the other role.
            string role = session.GetRole() ?? "";

            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                tempData["Error"] = "You do not have access to that page.";

                string controller = role == RoleNames.Organiser ? "Organiser" : "Participant";

                context.Result = new RedirectToActionResult("Index", controller, null);
            }
        }
    }
}
