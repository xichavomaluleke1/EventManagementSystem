using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models
{
    public class CategoryFormViewModel
    {
        public int EventId { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal? EntryFee { get; set; }
    }
}