using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Your name must be 100 characters or fewer.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please enter a password.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        // Leave empty to register as a Participant.
        public string? OrganiserCode { get; set; }
    }
}
