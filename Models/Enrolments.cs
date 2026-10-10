using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_management_system.Models
{//start of namespace
    public class Enrolments
    {//start of class

        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int ParticipantID { get; set; }

        [ForeignKey("ParticipantID")]
        public Users? Participant { get; set; }

        [Required]
        public int EventID { get; set; }

        [ForeignKey("EventID")]
        public Events? Event { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [ForeignKey("CategoryID")]
        public Categories? Category { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, etc.

        [Required]
        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, etc.

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }//end of class

}//end of namespace
