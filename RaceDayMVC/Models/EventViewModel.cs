using System;
using System.Collections.Generic;

namespace RaceDayMVC.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int? MaxParticipants { get; set; }
        public decimal? EntryFee { get; set; }
        public string? BannerImage { get; set; }
        public string Status { get; set; } = string.Empty;

        // Holds the categories for the event detail page
        public List<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
    }

    public class CategoryViewModel
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? EntryFee { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
    }
}