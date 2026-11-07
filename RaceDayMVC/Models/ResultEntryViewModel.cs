using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class ResultEntryViewModel
    {
        [Required]
        public int EventId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        [Required]
        public string FinishTime { get; set; } = string.Empty; // format: HH:mm:ss

        public int? FinishPosition { get; set; }
        public int? TotalParticipants { get; set; }

        public string Status { get; set; } = "Confirmed";
    }
}