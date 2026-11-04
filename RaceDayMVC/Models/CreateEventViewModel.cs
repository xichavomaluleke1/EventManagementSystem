using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDayMVC.Models
{
    public class CreateEventViewModel
    {
        [Required]
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = "Run"; // Run, Walk, Cycle

        public int? MaxParticipants { get; set; }
        public decimal? EntryFee { get; set; }

        // This handles the Azure Banner upload
        public IFormFile? BannerImage { get; set; }
    }
}