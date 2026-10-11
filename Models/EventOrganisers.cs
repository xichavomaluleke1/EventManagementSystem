using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_management_system.Models
{//start of namespacee
    public class EventOrganisers
    {//start of class

        [Key]
        public int EventOrganiserID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public Users? User { get; set; } // Navigation property

        [StringLength(255)]
        public string? OrganizationName { get; set; }

        [StringLength(50)]
        public string? OrganizationType { get; set; }

        [StringLength(255)]
        public string? Website { get; set; }

        [StringLength(20)]
        public string? ContactNumber { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }//end of class

}//end of namespace
