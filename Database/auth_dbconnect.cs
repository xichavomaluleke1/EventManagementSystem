using System.Data.SqlClient;
using event_management_system.Models;
using event_management_system.Database.Interfaces;

namespace event_management_system.Database
{//start of namespace

    public class auth_dbconnect : Iauth_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // ==================================================
        // STORE a new user (register)
        // ==================================================
        public string store_user_info(string email, string password_hash, string firstname, string surname,
            string role, DateTime? date_of_birth, string gender, string? phone_number)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"insert into users 
                                     (email, passwordHash, role, firstName, lastName, dateOfBirth, 
                                      gender, phoneNumber, createdAt, updateAt, isActive) 
                                     values 
                                     (@Email, @PasswordHash, @Role, @FirstName, @LastName, @DateOfBirth, 
                                      @Gender, @PhoneNumber, GETDATE(), GETDATE(), 1);";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@PasswordHash", password_hash);
                        cmd.Parameters.AddWithValue("@Role", role);
                        cmd.Parameters.AddWithValue("@FirstName", firstname);
                        cmd.Parameters.AddWithValue("@LastName", surname);
                        cmd.Parameters.AddWithValue("@DateOfBirth",
                            date_of_birth.HasValue ? (object)date_of_birth.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", gender);
                        cmd.Parameters.AddWithValue("@PhoneNumber",
                            string.IsNullOrWhiteSpace(phone_number) ? (object)DBNull.Value : phone_number);

                        cmd.ExecuteNonQuery();
                    }
                    connect.Close();
                    message = "User registered successfully.";
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // CHECK if email exists
        // ==================================================
        public bool email_exists(string email)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = "select count(*) from users where email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        connect.Close();
                        return count > 0;
                    }
                }
            }
            catch (Exception) { return false; }
        }

        // ==================================================
        // GET user by email
        // ==================================================
        public Users? get_user_by_email(string email)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select userID, email, passwordHash, role, firstName, lastName, 
                                            dateOfBirth, gender, phoneNumber, profile_picture_url,
                                            createdAt, updateAt, isActive, lastLoginDate 
                                     from users where email = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = map_user(reader);
                                connect.Close();
                                return user;
                            }
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return null; }
            return null;
        }

        // ==================================================
        // GET user by id
        // ==================================================
        public Users? get_user_by_id(int user_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select userID, email, passwordHash, role, firstName, lastName, 
                                            dateOfBirth, gender, phoneNumber, profile_picture_url,
                                            createdAt, updateAt, isActive, lastLoginDate 
                                     from users where userID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@UserID", user_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var user = map_user(reader);
                                connect.Close();
                                return user;
                            }
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return null; }
            return null;
        }

        // ==================================================
        // UPDATE last login
        // ==================================================
        public void update_last_login(int user_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = "update users set lastLoginDate = GETDATE() where userID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@UserID", user_id);
                        cmd.ExecuteNonQuery();
                    }
                    connect.Close();
                }
            }
            catch (Exception) { }
        }

        // ==================================================
        // MAPPER
        // ==================================================
        private Users map_user(SqlDataReader reader)
        {
            return new Users
            {
                UserID = Convert.ToInt32(reader["userID"]),
                Email = reader["email"].ToString() ?? "",
                PasswordHash = reader["passwordHash"].ToString() ?? "",
                Role = reader["role"].ToString() ?? "",
                FirstName = reader["firstName"].ToString() ?? "",
                LastName = reader["lastName"].ToString() ?? "",
                DateOfBirth = Convert.ToDateTime(reader["dateOfBirth"]),
                Gender = reader["gender"].ToString() ?? "",
                PhoneNumber = reader["phoneNumber"] == DBNull.Value ? null : reader["phoneNumber"].ToString(),
                ProfilePictureURL = reader["profile_picture_url"] == DBNull.Value ? null : reader["profile_picture_url"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                UpdatedAt = reader["updateAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["updateAt"]),
                IsActive = Convert.ToBoolean(reader["isActive"]),
                LastLoginDate = reader["lastLoginDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["lastLoginDate"])
            };
        }
    }
}