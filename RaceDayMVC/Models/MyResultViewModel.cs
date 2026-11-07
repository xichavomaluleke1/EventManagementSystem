namespace RaceDayMVC.Models
{
    public class MyResultViewModel
    {
        public int ResultID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string EventType { get; set; } = string.Empty;
        public TimeSpan? FinishTime { get; set; }
        public int? FinishPosition { get; set; }
        public int? TotalParticipants { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}