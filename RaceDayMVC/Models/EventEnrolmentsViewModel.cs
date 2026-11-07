namespace RaceDayMVC.Models
{
    public class EventEnrolmentsViewModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<EnrolmentDetail> Enrolments { get; set; } = new List<EnrolmentDetail>();
    }

    public class EnrolmentDetail
    {
        public int EnrolmentID { get; set; }
        public int ParticipantID { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }
}