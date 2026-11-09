namespace RaceDayMVC.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string Title { get; set; } = "Something went wrong";

        public string Message { get; set; } =
            "An unexpected problem occurred. Please try again.";
    }
}
