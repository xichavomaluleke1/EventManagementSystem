using System.Data.SqlClient;
using event_management_system.Models;

namespace event_management_system.Database
{//start of namespace

    public class users_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // ==================================================
        // GET user by id
        // ==================================================
        public Users? get_user_by_id(int user_id)
        {//start of get_user_by_id()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select userID, email, passwordHash, role, firstName, lastName, 
                                            dateOfBirth, gender, phoneNumber, profile_picture_url,
                                            createdAt, updateAt, isActive, lastLoginDate 
                                     from users 
                                     where userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@UserID", user_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {//start of if

                                var user = map_user(reader);
                                connect.Close();
                                return user;

                            }//end of if

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return null;

            }//end of catch

            return null;

        }//end of get_user_by_id()

        // ==================================================
        // GET user by email
        // ==================================================
        public Users? get_user_by_email(string email)
        {//start of get_user_by_email()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select userID, email, passwordHash, role, firstName, lastName, 
                                            dateOfBirth, gender, phoneNumber, profile_picture_url,
                                            createdAt, updateAt, isActive, lastLoginDate 
                                     from users 
                                     where email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {//start of if

                                var user = map_user(reader);
                                connect.Close();
                                return user;

                            }//end of if

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return null;

            }//end of catch

            return null;

        }//end of get_user_by_email()

        // ==================================================
        // UPDATE user profile
        // ==================================================
        public string update_user_profile(int user_id, string firstname, string surname,
            string? phone_number, DateTime? date_of_birth, string gender)
        {//start of update_user_profile()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update users 
                                     set firstName = @FirstName, 
                                         lastName = @LastName, 
                                         phoneNumber = @PhoneNumber, 
                                         dateOfBirth = @DateOfBirth, 
                                         gender = @Gender, 
                                         updateAt = GETDATE() 
                                     where userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@UserID", user_id);
                        cmd.Parameters.AddWithValue("@FirstName", firstname);
                        cmd.Parameters.AddWithValue("@LastName", surname);
                        cmd.Parameters.AddWithValue("@PhoneNumber",
                            string.IsNullOrWhiteSpace(phone_number) ? (object)DBNull.Value : phone_number);
                        cmd.Parameters.AddWithValue("@DateOfBirth",
                            date_of_birth.HasValue ? (object)date_of_birth.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Gender", gender);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Profile updated successfully.";
                        else
                            message = "User not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of update_user_profile()

        // ==================================================
        // GET profile picture URL
        // ==================================================
        public string? get_profile_picture(int user_id)
        {//start of get_profile_picture()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = "select profile_picture_url from users where userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@UserID", user_id);

                        var result = cmd.ExecuteScalar();

                        connect.Close();

                        if (result == null || result == DBNull.Value)
                            return null;

                        return result.ToString();

                    }//end of using

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return null;

            }//end of catch

        }//end of get_profile_picture()

        // ==================================================
        // UPDATE profile picture
        // ==================================================
        public string update_profile_picture(int user_id, string picture_url)
        {//start of update_profile_picture()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update users 
                                     set profile_picture_url = @PictureUrl, 
                                         updateAt = GETDATE() 
                                     where userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@UserID", user_id);
                        cmd.Parameters.AddWithValue("@PictureUrl", picture_url);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Profile picture updated successfully.";
                        else
                            message = "User not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of update_profile_picture()

        // ==================================================
        // UPDATE password
        // ==================================================
        public string update_password(int user_id, string new_password_hash)
        {//start of update_password()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update users 
                                     set passwordHash = @PasswordHash, 
                                         updateAt = GETDATE() 
                                     where userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@UserID", user_id);
                        cmd.Parameters.AddWithValue("@PasswordHash", new_password_hash);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Password changed successfully.";
                        else
                            message = "User not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of update_password()

        // ==================================================
        // MAPPER
        // ==================================================
        private Users map_user(SqlDataReader reader)
        {//start of map_user()

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

        }//end of map_user()

    }//end of class

}//end of namespace