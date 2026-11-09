namespace RaceDayMVC.Models
{
    // Plain data shapes returned by / sent to the RaceDay API.

    public class LoginResponseDto
    {
        public string Message { get; set; } = "";
        public string Token { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }
    }

    public class RaceEventDto
    {
        public int EventID { get; set; }
        public int OrganizerID { get; set; }
        public int CategoryID { get; set; }
        public int RouteID { get; set; }
        public string EventName { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public decimal EntryFee { get; set; }
        public string Status { get; set; } = "Open";
        public string? BannerUrl { get; set; }
    }

    // The event type (Running, Walking, Cycling ...)
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
        public string? Description { get; set; }
    }

    public class RouteDto
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public string StartPoint { get; set; } = "";
        public string FinishPoint { get; set; } = "";
        public string? Description { get; set; }
    }

    // An age or distance category that belongs to one event.
    public class EventCategoryDto
    {
        public int EventCategoryID { get; set; }
        public int EventID { get; set; }
        public string Name { get; set; } = "";
        public string CategoryType { get; set; } = "Distance";
        public string? Description { get; set; }
    }

    public class RegistrationDto
    {
        public int RegistrationID { get; set; }
        public int EventID { get; set; }
        public int ParticipantID { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int? RaceNumber { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public decimal? AveragePace { get; set; }
        public string? ResultStatus { get; set; }
        public int? EventCategoryID { get; set; }
    }

    // One row of the organiser's enrolment list.
    public class EnrolmentDetailDto
    {
        public int RegistrationID { get; set; }
        public int EventID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = "";
        public string ParticipantEmail { get; set; } = "";
        public int? EventCategoryID { get; set; }
        public string? CategoryName { get; set; }
        public string PaymentStatus { get; set; } = "";
        public DateTime RegistrationDate { get; set; }
        public int? RaceNumber { get; set; }
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public decimal? AveragePace { get; set; }
        public string? ResultStatus { get; set; }
    }

    public class ProfileDto
    {
        public int UserAccountID { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }
    }

    public class UploadResultDto
    {
        public string Url { get; set; } = "";
    }

    // The outcome of an API call, with a friendly message when it fails.
    public class ApiResult<T>
    {
        public bool Success { get; private set; }
        public T? Data { get; private set; }
        public string Error { get; private set; } = "";
        public bool Unauthorized { get; private set; }

        public static ApiResult<T> Ok(T data)
        {
            return new ApiResult<T> { Success = true, Data = data };
        }

        public static ApiResult<T> Fail(string error, bool unauthorized = false)
        {
            return new ApiResult<T> { Success = false, Error = error, Unauthorized = unauthorized };
        }
    }
}
