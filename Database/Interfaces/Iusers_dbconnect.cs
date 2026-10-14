using event_management_system.Models;

namespace event_management_system.Database.Interfaces
{
    public interface Iusers_dbconnect
    {
        Users? get_user_by_id(int user_id);

        string update_user_profile(int user_id, string firstname, string surname,
            string? phone_number, DateTime? date_of_birth, string gender);

        string? get_profile_picture(int user_id);

        string update_profile_picture(int user_id, string picture_url);

        string update_password(int user_id, string new_password_hash);
    }
}