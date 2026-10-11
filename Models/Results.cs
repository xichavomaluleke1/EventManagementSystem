using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_management_system.Models
{//start of namespace
    public class Results
    {//start of class

        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [ForeignKey("EnrolmentID")]
        public Enrolments? Enrolment { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public int? FinishPosition { get; set; }
        public int? TotalParticipants { get; set; }
        public int? CategoryPosition { get; set; }
        public int? TotalCategoryParticipants { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "DNS"; // Pending, Confirmed, DNS, DNF, Disqualified

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        [Required]
        public int RecordedBy { get; set; }

        [ForeignKey("RecordedBy")]
        public Users? Recorder { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

    }//end of class

}//end of namespace
