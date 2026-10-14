namespace Part_2.Models
{
    public class UserAccount
    {
            public int UserAccountID { get; set; }

            public string FullName { get; set; } = string.Empty;

            public string Email { get; set; } = string.Empty;

            // Store a password hash, never the plain password
            public string PasswordHash { get; set; } = string.Empty;

            // Examples: Admin, Organizer, Participant
            public string Role { get; set; } = "Participant";
        }
    }

}
}
