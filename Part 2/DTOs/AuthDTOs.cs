using Microsoft.VisualBasic;
using System.ComponentModel;

namespace Part_2.DTOs
{//Start of namespace

    // Contains the information needed to register an account.
    public class RegisterRequest
    {//Start of class

        // The name entered by the new user.
        public string Name { get; set; } = string.Empty;

        // The email used to identify the account.
        public string Email { get; set; } = string.Empty;

        // The password supplied during registration.
        public string Password { get; set; } = string.Empty;

        // New accounts should receive the Participant role by default.
        public string Role { get; set; } = "Participant";


    }//End of class

    // Contains the information needed to log in.
    public class LoginRequest
    {//Start of class

        // The email associated with the account.
        public string Email { get; set; } = string.Empty;

        // The password entered by the user.
        public string Password { get; set; } = string.Empty;


    }//End of class
}//End of namespace


