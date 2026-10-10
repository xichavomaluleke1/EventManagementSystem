using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_management_system.Models
{//start of namespace
    public class Events
    {//start of class

        [Key]
        public int EventID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        [ForeignKey("OrganiserID")]
        public Users? Organiser { get; set; }

        [Required]
        [StringLength(255)]
        public string EventName { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Location { get; set; }

        [Required]
        public decimal Distance { get; set; }

        [Required]
        [StringLength(20)]
        public string EventType { get; set; }

        public int? MaxParticipants { get; set; }

        public decimal? EntryFee { get; set; }

        [StringLength(500)]
        public string? BannerImage { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Upcoming";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool? IsPublished { get; set; } = true;

    }//end of class

}//end of namespace
