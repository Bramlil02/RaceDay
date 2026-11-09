using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RaceDayMVC.Models;

namespace RaceDayMVC.Helpers
{
    // The keys used to store the logged-in user in the session.
    public static class SessionKeys
    {
        public const string Token = "JWToken";
        public const string FullName = "FullName";
        public const string Email = "Email";
        public const string Role = "Role";
        public const string Picture = "ProfilePictureUrl";
    }

    public static class RoleNames
    {
        public const string Organiser = "Organiser";
        public const string Participant = "Participant";

        // The API calls organisers "Organizer" (and has an Admin role).
        // The MVC application uses two roles only.
        public static string Normalise(string? apiRole)
        {
            string role = (apiRole ?? "").Trim();

            if (role.Equals("Organizer", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Organiser", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return Organiser;
            }

            if (role.Equals("Participant", StringComparison.OrdinalIgnoreCase))
                return Participant;

            return "";
        }
    }

    public static class SessionHelper
    {
        public static bool IsLoggedIn(this ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString(SessionKeys.Token));
        }

        public static string? GetRole(this ISession session)
        {
            return session.GetString(SessionKeys.Role);
        }

        // Saves the user's details after a successful login.
        public static void SignIn(this ISession session, LoginResponseDto login, string role)
        {
            session.Clear();
            session.SetString(SessionKeys.Token, login.Token);
            session.SetString(SessionKeys.FullName, login.FullName);
            session.SetString(SessionKeys.Email, login.Email);
            session.SetString(SessionKeys.Role, role);
            session.SetString(SessionKeys.Picture, login.ProfilePictureUrl ?? "");
        }

        // The API token lasts two hours; read its expiry date from the token.
        public static bool IsTokenExpired(this ISession session)
        {
            string? token = session.GetString(SessionKeys.Token);

            if (string.IsNullOrEmpty(token))
                return true;

            try
            {
                string[] parts = token.Split('.');

                if (parts.Length < 2)
                    return false;

                string payload = parts[1].Replace('-', '+').Replace('_', '/');

                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                string json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

                using var document = JsonDocument.Parse(json);

                if (document.RootElement.TryGetProperty("exp", out JsonElement exp) &&
                    exp.TryGetInt64(out long seconds))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(seconds) <= DateTimeOffset.UtcNow;
                }
            }
            catch (Exception)
            {
                // If the token cannot be read, let the API decide.
            }

            return false;
        }
    }
}
