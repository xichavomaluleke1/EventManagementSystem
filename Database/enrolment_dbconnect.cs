using System.Data.SqlClient;
using event_management_system.Models;

namespace event_management_system.Database
{//start of namespace

    public class enrolment_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // ==================================================
        // STORE a new enrolment
        // ==================================================
        public string store_enrolment_info(int participant_id, int event_id, int category_id)
        {//start of store_enrolment_info()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    // duplicate check — prevent the same participant enrolling twice
                    string check_query = @"select count(*) from enrolments 
                                           where ParticipantID = @ParticipantID 
                                           and EventID = @EventID 
                                           and Status != 'Cancelled'";

                    using (SqlCommand check_cmd = new SqlCommand(check_query, connect))
                    {//start of using

                        check_cmd.Parameters.AddWithValue("@ParticipantID", participant_id);
                        check_cmd.Parameters.AddWithValue("@EventID", event_id);

                        int exists = Convert.ToInt32(check_cmd.ExecuteScalar());

                        if (exists > 0)
                        {
                            connect.Close();
                            return "You are already enrolled in this event.";
                        }

                    }//end of using

                    // insert
                    string query = @"insert into enrolments 
                                     (ParticipantID, EventID, CategoryID, EnrolmentDate, Status, 
                                      PaymentStatus, CreatedAt, UpdatedAt) 
                                     values 
                                     (@ParticipantID, @EventID, @CategoryID, GETDATE(), 'Pending', 
                                      'Pending', GETDATE(), GETDATE());";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@ParticipantID", participant_id);
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        cmd.Parameters.AddWithValue("@CategoryID", category_id);

                        cmd.ExecuteNonQuery();

                    }//end of using

                    connect.Close();
                    message = "Enrolment created successfully.";

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of store_enrolment_info()

        // ==================================================
        // GET all enrolments (optionally filtered by event)
        // ==================================================
        public List<Enrolments> get_all_enrolments(int? event_id = null)
        {//start of get_all_enrolments()

            List<Enrolments> enrolments = new List<Enrolments>();

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments";

                    if (event_id.HasValue)
                        query += " where EventID = @EventID";

                    query += " order by EnrolmentDate desc";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        if (event_id.HasValue)
                            cmd.Parameters.AddWithValue("@EventID", event_id.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            while (reader.Read())
                            {//start of while

                                enrolments.Add(map_enrolment(reader));

                            }//end of while

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return new List<Enrolments>();

            }//end of catch

            return enrolments;

        }//end of get_all_enrolments()

        // ==================================================
        // GET enrolments by participant
        // ==================================================
        public List<Enrolments> get_enrolments_by_participant(int participant_id)
        {//start of get_enrolments_by_participant()

            List<Enrolments> enrolments = new List<Enrolments>();

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments 
                                     where ParticipantID = @ParticipantID
                                     order by EnrolmentDate desc";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@ParticipantID", participant_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            while (reader.Read())
                            {//start of while

                                enrolments.Add(map_enrolment(reader));

                            }//end of while

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return new List<Enrolments>();

            }//end of catch

            return enrolments;

        }//end of get_enrolments_by_participant()

        // ==================================================
        // GET enrolments by event
        // ==================================================
        public List<Enrolments> get_enrolments_by_event(int event_id)
        {//start of get_enrolments_by_event()

            List<Enrolments> enrolments = new List<Enrolments>();

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments 
                                     where EventID = @EventID
                                     order by EnrolmentDate desc";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EventID", event_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            while (reader.Read())
                            {//start of while

                                enrolments.Add(map_enrolment(reader));

                            }//end of while

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return new List<Enrolments>();

            }//end of catch

            return enrolments;

        }//end of get_enrolments_by_event()

        // ==================================================
        // GET enrolment by id
        // ==================================================
        public Enrolments? get_enrolment_by_id(int enrolment_id)
        {//start of get_enrolment_by_id()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select EnrolmentID, ParticipantID, EventID, CategoryID, EnrolmentDate, 
                                            Status, PaymentStatus, paymentReference, Notes, CreatedAt, UpdatedAt 
                                     from enrolments 
                                     where EnrolmentID = @EnrolmentID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EnrolmentID", enrolment_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {//start of if

                                var enr = map_enrolment(reader);
                                connect.Close();
                                return enr;

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

        }//end of get_enrolment_by_id()

        // ==================================================
        // UPDATE enrolment status (organiser)
        // ==================================================
        public string update_enrolment_status(int enrolment_id, string status, string payment_status)
        {//start of update_enrolment_status()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update enrolments 
                                     set Status = @Status, 
                                         PaymentStatus = @PaymentStatus, 
                                         UpdatedAt = GETDATE() 
                                     where EnrolmentID = @EnrolmentID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EnrolmentID", enrolment_id);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@PaymentStatus", payment_status);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Enrolment status updated.";
                        else
                            message = "Enrolment not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of update_enrolment_status()

        // ==================================================
        // CANCEL enrolment (participant)
        // ==================================================
        public string cancel_enrolment(int enrolment_id)
        {//start of cancel_enrolment()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update enrolments 
                                     set Status = 'Cancelled', 
                                         UpdatedAt = GETDATE() 
                                     where EnrolmentID = @EnrolmentID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EnrolmentID", enrolment_id);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Enrolment cancelled.";
                        else
                            message = "Enrolment not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of cancel_enrolment()

        // ==================================================
        // MAPPER
        // ==================================================
        private Enrolments map_enrolment(SqlDataReader reader)
        {//start of map_enrolment()

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

        }//end of map_enrolment()

    }//end of class

}//end of namespace