namespace event_management_system.Models
{
    public class EventOrganisers
    {
        public int EventOrganiserID { get; set; }
        public int UserID { get; set; }
        public string? OrganizationName { get; set; }
        public string? OrganizationType { get; set; }
        public string? Website { get; set; }
        public string? ContactNumber { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}