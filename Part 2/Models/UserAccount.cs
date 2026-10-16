namespace Part_2.Models
{//Start of namespace
    public class UserAccount
    {//Start of class

        // Unique ID for each user account.
        public int UserAccountID { get; set; }

        // Stores the user's full name.
        public string FullName { get; set; } = string.Empty;

        // Stores the user's email address.
        public string Email { get; set; } = string.Empty;

        // Store a password hash, never the plain password
        public string PasswordHash { get; set; } = string.Empty;

        // Identifies the user's permissions in the system.
        // Examples include Admin, Organizer and Participant.
        public string Role { get; set; } = "Participant";


        }//End of class
    }//End of namespace