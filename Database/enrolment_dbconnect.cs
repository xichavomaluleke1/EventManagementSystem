using System.Data.SqlClient;
using event_management_system.Models;
using event_management_system.Database.Interfaces;

namespace event_management_system.Database
{//start of namespace

    public class enrolment_dbconnect : Ienrolment_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // ==================================================
        // STORE new enrolment
        // ==================================================
        public string store_enrolment_info(int participant_id, int event_id, int category_id)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();

                    // duplicate check
                    string check_query = @"select count(*) from enrolments 
                                           where ParticipantID = @ParticipantID 
                                           and EventID = @EventID and Status != 'Cancelled'";
                    using (SqlCommand check_cmd = new SqlCommand(check_query, connect))
                    {
                        check_cmd.Parameters.AddWithValue("@ParticipantID", participant_id);
                        check_cmd.Parameters.AddWithValue("@EventID", event_id);
                        int exists = Convert.ToInt32(check_cmd.ExecuteScalar());
                        if (exists > 0)
                        {
                            connect.Close();
                            return "You are already enrolled in this event.";
                        }
                    }

                    string query = @"insert into enrolments 
                                     (ParticipantID, EventID, CategoryID, EnrolmentDate, Status, 
                                      PaymentStatus, CreatedAt, UpdatedAt) 
                                     values 
                                     (@ParticipantID, @EventID, @CategoryID, GETDATE(), 'Pending', 
                                      'Pending', GETDATE(), GETDATE());";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ParticipantID", participant_id);
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        cmd.Parameters.AddWithValue("@CategoryID", category_id);
                        cmd.ExecuteNonQuery();
                    }
                    connect.Close();
                    message = "Enrolment created successfully.";
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // GET all enrolments (optionally by event)
        // ==================================================
        public List<Enrolments> get_all_enrolments(int? event_id = null)
        {
            List<Enrolments> enrolments = new List<Enrolments>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments";
                    if (event_id.HasValue) query += " where EventID = @EventID";
                    query += " order by EnrolmentDate desc";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        if (event_id.HasValue) cmd.Parameters.AddWithValue("@EventID", event_id.Value);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) enrolments.Add(map_enrolment(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<Enrolments>(); }
            return enrolments;
        }

        // ==================================================
        // GET by participant
        // ==================================================
        public List<Enrolments> get_enrolments_by_participant(int participant_id)
        {
            List<Enrolments> enrolments = new List<Enrolments>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments where ParticipantID = @ParticipantID 
                                     order by EnrolmentDate desc";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ParticipantID", participant_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) enrolments.Add(map_enrolment(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<Enrolments>(); }
            return enrolments;
        }

        // ==================================================
        // GET by event
        // ==================================================
        public List<Enrolments> get_enrolments_by_event(int event_id)
        {
            List<Enrolments> enrolments = new List<Enrolments>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments where EventID = @EventID 
                                     order by EnrolmentDate desc";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) enrolments.Add(map_enrolment(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<Enrolments>(); }
            return enrolments;
        }

        // ==================================================
        // GET by id
        // ==================================================
        public Enrolments? get_enrolment_by_id(int enrolment_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments where EnrolmentID = @EnrolmentID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EnrolmentID", enrolment_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var enr = map_enrolment(reader);
                                connect.Close();
                                return enr;
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
        // UPDATE status (organiser)
        // ==================================================
        public string update_enrolment_status(int enrolment_id, string status, string payment_status)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"update enrolments 
                                     set Status = @Status, PaymentStatus = @PaymentStatus, 
                                         UpdatedAt = GETDATE() 
                                     where EnrolmentID = @EnrolmentID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EnrolmentID", enrolment_id);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@PaymentStatus", payment_status);
                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Enrolment status updated." : "Enrolment not found.";
                    }
                    connect.Close();
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // CANCEL (participant)
        // ==================================================
        public string cancel_enrolment(int enrolment_id)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"update enrolments 
                                     set Status = 'Cancelled', UpdatedAt = GETDATE() 
                                     where EnrolmentID = @EnrolmentID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EnrolmentID", enrolment_id);
                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Enrolment cancelled." : "Enrolment not found.";
                    }
                    connect.Close();
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // MAPPER
        // ==================================================
        private Enrolments map_enrolment(SqlDataReader reader)
        {
            return new Enrolments
            {
                EnrolmentID = Convert.ToInt32(reader["EnrolmentID"]),
                ParticipantID = Convert.ToInt32(reader["ParticipantID"]),
                EventID = Convert.ToInt32(reader["EventID"]),
                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                EnrolmentDate = Convert.ToDateTime(reader["EnrolmentDate"]),
                Status = reader["Status"].ToString() ?? "",
                PaymentStatus = reader["PaymentStatus"].ToString() ?? "",
                PaymentReference = reader["paymentReference"] == DBNull.Value ? null : reader["paymentReference"].ToString(),
                Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["UpdatedAt"])
            };
        }
    }
}