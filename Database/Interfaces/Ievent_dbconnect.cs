using event_management_system.Models;

namespace event_management_system.Database.Interfaces
{
    public interface Ievent_dbconnect
    {
        string store_event_info(int organiser_id, string event_name, string? description,
            DateTime event_date, string location, decimal distance, string event_type,
            int? max_participants, decimal? entry_fee, string status);

        List<Events> get_all_events();

        List<Events> get_events_by_organiser(int organiser_id);

        Events? get_event_by_id(int event_id);

        string update_event_info(int event_id, string event_name, string? description,
            DateTime event_date, string location, decimal distance, string event_type,
            int? max_participants, decimal? entry_fee, string status);

        string delete_event(int event_id);

        string publish_event(int event_id, bool is_published);
    }
}