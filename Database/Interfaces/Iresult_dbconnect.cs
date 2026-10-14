using event_management_system.Models;
using ResultModel = event_management_system.Models.Results;

namespace event_management_system.Database.Interfaces
{
    public interface Iresult_dbconnect
    {
        string store_result_info(int enrolment_id, TimeSpan? finish_time, int? finish_position,
            int? total_participants, int? category_position, int? total_category_participants,
            string status, int recorded_by, string? notes);

        List<ResultModel> get_all_results(int? event_id = null);

        List<ResultModel> get_results_by_participant(int participant_id);

        List<ResultModel> get_results_by_event(int event_id);

        ResultModel? get_result_by_id(int result_id);

        string update_result_info(int result_id, TimeSpan? finish_time, int? finish_position,
            int? total_participants, int? category_position, int? total_category_participants,
            string status, string? notes);

        string delete_result(int result_id);
    }
}