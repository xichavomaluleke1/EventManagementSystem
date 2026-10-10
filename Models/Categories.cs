using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace event_management_system.Models
{//start of namespace
    public class Categories
    {//start of class

        [Key]
        public int CategoryID { get; set; }

        [Required]
        public int EventID { get; set; }

        [ForeignKey("EventID")]
        public Events? Event { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        public decimal? MinDistance { get; set; }
        public decimal? MaxDistance { get; set; }

        public decimal? EntryFee { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool? IsActive { get; set; } = true;

    }//end of class

}//end of namespace
