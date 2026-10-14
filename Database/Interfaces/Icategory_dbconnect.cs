using event_management_system.Models;

namespace event_management_system.Database.Interfaces
{
    public interface Icategory_dbconnect
    {
        string store_category_info(int event_id, string category_name, string? description,
            int? min_age, int? max_age, decimal? min_distance, decimal? max_distance, decimal? entry_fee);

        List<Categories> get_all_categories(int? event_id = null);

        List<Categories> get_categories_by_event(int event_id);

        Categories? get_category_by_id(int category_id);

        string update_category_info(int category_id, string category_name, string? description,
            int? min_age, int? max_age, decimal? min_distance, decimal? max_distance,
            decimal? entry_fee, bool? is_active);

        string delete_category(int category_id);
    }
}