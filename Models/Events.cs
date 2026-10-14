namespace event_management_system.Models
{
    public class Events
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
        public string Status { get; set; } = "Upcoming";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool? IsPublished { get; set; }
    }
}