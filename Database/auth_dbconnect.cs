using System.Data.SqlClient;
using event_management_system.Models;

namespace event_management_system.Database
{//start of namespace

    public class auth_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        //REGISTER - store a new user
        public string store_user_info(string email, string password_hash, string firstname, string surname,
            string role, DateTime? date_of_birth, string gender, string? phone_number)
        {//start of store_user_info()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"insert into users 
                                     (email, passwordHash, role, firstName, lastName, dateOfBirth, 
                                      gender, phoneNumber, createdAt, updateAt, isActive) 
                                     values 
                                     (@Email, @PasswordHash, @Role, @FirstName, @LastName, @DateOfBirth, 
                                      @Gender, @PhoneNumber, GETDATE(), GETDATE(), 1);";

                    using (SqlCommand run_query = new SqlCommand(query, connect))
                    {//start of using

                        run_query.Parameters.AddWithValue("@Email", email);
                        run_query.Parameters.AddWithValue("@PasswordHash", password_hash);
                        run_query.Parameters.AddWithValue("@Role", role);
                        run_query.Parameters.AddWithValue("@FirstName", firstname);
                        run_query.Parameters.AddWithValue("@LastName", surname);
                        run_query.Parameters.AddWithValue("@DateOfBirth",
                            date_of_birth.HasValue ? (object)date_of_birth.Value : DBNull.Value);
                        run_query.Parameters.AddWithValue("@Gender", gender);
                        run_query.Parameters.AddWithValue("@PhoneNumber",
                            string.IsNullOrWhiteSpace(phone_number) ? (object)DBNull.Value : phone_number);

                        run_query.ExecuteNonQuery();

                    }//end of using

                    connect.Close();

                    message = "User registered successfully.";

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of store_user_info()

        //LOGIN HELPERS
        //method to check if an email exists
        public bool email_exists(string email)
        {//start of email_exists()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = "select count(*) from users where email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@Email", email);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        connect.Close();

                        return count > 0;

                    }//end of using

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return false;

            }//end of catch

        }//end of email_exists()

        //method to fetch a user by email
        public Users? get_user_by_email(string email)
        {//start of get_user_by_email()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select userID, email, passwordHash, role, firstName, lastName, 
                                            dateOfBirth, gender, phoneNumber, createdAt, updateAt, 
                                            isActive, lastLoginDate 
                                     from users 
                                     where email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {//start of if

                                var user = new Users
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
                                    CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                                    UpdatedAt = reader["updateAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["updateAt"]),
                                    IsActive = Convert.ToBoolean(reader["isActive"]),
                                    LastLoginDate = reader["lastLoginDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["lastLoginDate"])
                                };

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

        //method to fetch a user by id
        public Users? get_user_by_id(int user_id)
        {//start of get_user_by_id()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select userID, email, passwordHash, role, firstName, lastName, 
                                            dateOfBirth, gender, phoneNumber, createdAt, updateAt, 
                                            isActive, lastLoginDate 
                                     from users 
                                     where userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@UserID", user_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {//start of if

                                var user = new Users
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
                                    CreatedAt = Convert.ToDateTime(reader["createdAt"]),
                                    UpdatedAt = reader["updateAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["updateAt"]),
                                    IsActive = Convert.ToBoolean(reader["isActive"]),
                                    LastLoginDate = reader["lastLoginDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["lastLoginDate"])
                                };

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

        // UPDATE HELPERS
        //method to update the last login date
        public void update_last_login(int user_id)
        {//start of update_last_login()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = "update users set lastLoginDate = GETDATE() where userID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@UserID", user_id);
                        cmd.ExecuteNonQuery();

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch
                //silent fail - logging in shouldn't break if this fails
            }//end of catch

        }//end of update_last_login()

        //method to update user profile info
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

    }//end of class

}//end of namespace