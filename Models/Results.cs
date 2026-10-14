namespace event_management_system.Models
{
    public class Results
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? FinishPosition { get; set; }
        public int? TotalParticipants { get; set; }
        public int? CategoryPosition { get; set; }
        public int? TotalCategoryParticipants { get; set; }
        public string Status { get; set; } = "DNS";
        public DateTime RecordedAt { get; set; }
        public int RecordedBy { get; set; }
        public string? Notes { get; set; }
    }
}