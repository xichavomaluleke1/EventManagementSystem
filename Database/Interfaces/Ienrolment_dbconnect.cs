using event_management_system.Models;

namespace event_management_system.Database.Interfaces
{
    public interface Ienrolment_dbconnect
    {
        string store_enrolment_info(int participant_id, int event_id, int category_id);

        List<Enrolments> get_all_enrolments(int? event_id = null);

        List<Enrolments> get_enrolments_by_participant(int participant_id);

        List<Enrolments> get_enrolments_by_event(int event_id);

        Enrolments? get_enrolment_by_id(int enrolment_id);

        string update_enrolment_status(int enrolment_id, string status, string payment_status);

        string cancel_enrolment(int enrolment_id);
    }
}