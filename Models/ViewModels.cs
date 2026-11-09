using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RaceDayMVC.Models
{
    // ---------- Participant ----------

    public class EventCardViewModel
    {
        public RaceEventDto Event { get; set; } = new();
        public string TypeName { get; set; } = "";
        public decimal? DistanceKm { get; set; }
        public bool IsEnrolled { get; set; }
    }

    public class ParticipantHomeViewModel
    {
        public List<EventCardViewModel> Events { get; set; } = new();
        public List<CategoryDto> EventTypes { get; set; } = new();
        public string? Search { get; set; }
        public int? TypeId { get; set; }
        public string? Distance { get; set; }
        public string? Period { get; set; }
        public string? LoadError { get; set; }
    }

    public class EventDetailViewModel
    {
        public RaceEventDto Event { get; set; } = new();
        public string TypeName { get; set; } = "";
        public RouteDto? Route { get; set; }
        public List<EventCategoryDto> Categories { get; set; } = new();
        public RegistrationDto? MyEnrolment { get; set; }
        public string? MyCategoryName { get; set; }
        public bool CanEnrol { get; set; }
    }

    public class EnrolmentRowViewModel
    {
        public RegistrationDto Registration { get; set; } = new();
        public RaceEventDto? Event { get; set; }
        public string CategoryName { get; set; } = "";
        public bool CanCancel { get; set; }
    }

    public class ResultRowViewModel
    {
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public string FinishTime { get; set; } = "";
        public int? Position { get; set; }
        public int TotalFinishers { get; set; }
        public string ResultStatus { get; set; } = "";
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Your name must be 100 characters or fewer.")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";

        public string? ProfilePictureUrl { get; set; }
    }

    // ---------- Organiser ----------

    public class EventSummaryRow
    {
        public RaceEventDto Event { get; set; } = new();
        public int EnrolmentCount { get; set; }
        public string TypeName { get; set; } = "";
        public decimal? DistanceKm { get; set; }
    }

    public class OrganiserDashboardViewModel
    {
        public string OrganiserName { get; set; } = "";
        public List<EventSummaryRow> Events { get; set; } = new();
        public int TotalEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public int UpcomingEvents { get; set; }
    }

    public class EventFormViewModel
    {
        public int EventID { get; set; }
        public int OrganizerID { get; set; }
        public int RouteID { get; set; }
        public string? ExistingBannerUrl { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        [StringLength(150, ErrorMessage = "The name must be 150 characters or fewer.")]
        [Display(Name = "Event name")]
        public string EventName { get; set; } = "";

        [Required(ErrorMessage = "Please enter a description.")]
        [StringLength(1000, ErrorMessage = "The description must be 1000 characters or fewer.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose the event date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event date")]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Please enter the location.")]
        [StringLength(200, ErrorMessage = "The location must be 200 characters or fewer.")]
        public string Location { get; set; } = "";

        [Required(ErrorMessage = "Please enter the distance in kilometres.")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public decimal? DistanceKm { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public int? CategoryID { get; set; }

        [Range(0, 100000, ErrorMessage = "Entry fee must be between 0 and 100000.")]
        [Display(Name = "Entry fee (R)")]
        public decimal EntryFee { get; set; }

        public string Status { get; set; } = "Open";

        [Display(Name = "Event banner")]
        public IFormFile? Banner { get; set; }

        // Used to fill the drop-down; not posted back.
        public List<SelectListItem> EventTypeItems { get; set; } = new();
    }

    public class EventCategoriesViewModel
    {
        public RaceEventDto Event { get; set; } = new();
        public List<EventCategoryDto> Categories { get; set; } = new();
    }

    public class CategoryFormModel
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Please enter a category name.")]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [Required]
        public string CategoryType { get; set; } = "Distance";

        [StringLength(300)]
        public string? Description { get; set; }
    }

    public class EventEnrolmentsViewModel
    {
        public RaceEventDto Event { get; set; } = new();
        public List<EnrolmentDetailDto> Enrolments { get; set; } = new();
    }

    public class ResultRowInput
    {
        public int RegistrationID { get; set; }
        public string ParticipantName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public string? FinishTime { get; set; }
        public int? Position { get; set; }
        public string ResultStatus { get; set; } = "Completed";
    }

    public class ResultsEntryViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public decimal? DistanceKm { get; set; }
        public List<ResultRowInput> Rows { get; set; } = new();
    }
}
