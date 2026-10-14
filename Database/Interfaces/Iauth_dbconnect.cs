using event_management_system.Models;

namespace event_management_system.Database.Interfaces
{
    public interface Iauth_dbconnect
    {
        string store_user_info(string email, string password_hash, string firstname, string surname,
            string role, DateTime? date_of_birth, string gender, string? phone_number);

        bool email_exists(string email);

        Users? get_user_by_email(string email);

        Users? get_user_by_id(int user_id);

        void update_last_login(int user_id);
    }
}