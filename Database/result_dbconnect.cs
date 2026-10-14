using System.Data.SqlClient;
using event_management_system.Database.Interfaces;
using ResultModel = event_management_system.Models.Results;

namespace event_management_system.Database
{//start of namespace

    public class result_dbconnect : Iresult_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // ==================================================
        // STORE
        // ==================================================
        public string store_result_info(int enrolment_id, TimeSpan? finish_time, int? finish_position,
            int? total_participants, int? category_position, int? total_category_participants,
            string status, int recorded_by, string? notes)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"insert into results 
                                     (EnrolmentID, FinishTime, FinishPosition, TotalParticipants, 
                                      CategoryPosition, TotalCategoryParticipants, Status, 
                                      RecordedAt, RecordedBy, Notes) 
                                     values 
                                     (@EnrolmentID, @FinishTime, @FinishPosition, @TotalParticipants, 
                                      @CategoryPosition, @TotalCategoryParticipants, @Status, 
                                      GETDATE(), @RecordedBy, @Notes);";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EnrolmentID", enrolment_id);
                        cmd.Parameters.AddWithValue("@FinishTime", finish_time.HasValue ? (object)finish_time.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FinishPosition", finish_position.HasValue ? (object)finish_position.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalParticipants", total_participants.HasValue ? (object)total_participants.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CategoryPosition", category_position.HasValue ? (object)category_position.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalCategoryParticipants", total_category_participants.HasValue ? (object)total_category_participants.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@RecordedBy", recorded_by);
                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                        cmd.ExecuteNonQuery();
                    }
                    connect.Close();
                    message = "Result recorded successfully.";
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // GET all results (optionally by event)
        // ==================================================
        public List<ResultModel> get_all_results(int? event_id = null)
        {
            List<ResultModel> results = new List<ResultModel>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select r.ResultID, r.EnrolmentID, r.FinishTime, r.FinishPosition, 
                                            r.TotalParticipants, r.CategoryPosition, r.TotalCategoryParticipants, 
                                            r.Status, r.RecordedAt, r.RecordedBy, r.Notes 
                                     from results r";
                    if (event_id.HasValue)
                        query += @" inner join enrolments e on r.EnrolmentID = e.EnrolmentID 
                                    where e.EventID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        if (event_id.HasValue) cmd.Parameters.AddWithValue("@EventID", event_id.Value);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) results.Add(map_result(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<ResultModel>(); }
            return results;
        }

        // ==================================================
        // GET by participant
        // ==================================================
        public List<ResultModel> get_results_by_participant(int participant_id)
        {
            List<ResultModel> results = new List<ResultModel>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select r.ResultID, r.EnrolmentID, r.FinishTime, r.FinishPosition, 
                                            r.TotalParticipants, r.CategoryPosition, r.TotalCategoryParticipants, 
                                            r.Status, r.RecordedAt, r.RecordedBy, r.Notes 
                                     from results r 
                                     inner join enrolments e on r.EnrolmentID = e.EnrolmentID 
                                     where e.ParticipantID = @ParticipantID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ParticipantID", participant_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) results.Add(map_result(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<ResultModel>(); }
            return results;
        }

        // ==================================================
        // GET by event
        // ==================================================
        public List<ResultModel> get_results_by_event(int event_id)
        {
            List<ResultModel> results = new List<ResultModel>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select r.ResultID, r.EnrolmentID, r.FinishTime, r.FinishPosition, 
                                            r.TotalParticipants, r.CategoryPosition, r.TotalCategoryParticipants, 
                                            r.Status, r.RecordedAt, r.RecordedBy, r.Notes 
                                     from results r 
                                     inner join enrolments e on r.EnrolmentID = e.EnrolmentID 
                                     where e.EventID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) results.Add(map_result(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<ResultModel>(); }
            return results;
        }

        // ==================================================
        // GET by id
        // ==================================================
        public ResultModel? get_result_by_id(int result_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select ResultID, EnrolmentID, FinishTime, FinishPosition, 
                                            TotalParticipants, CategoryPosition, TotalCategoryParticipants, 
                                            Status, RecordedAt, RecordedBy, Notes 
                                     from results where ResultID = @ResultID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ResultID", result_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var res = map_result(reader);
                                connect.Close();
                                return res;
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
        // UPDATE
        // ==================================================
        public string update_result_info(int result_id, TimeSpan? finish_time, int? finish_position,
            int? total_participants, int? category_position, int? total_category_participants,
            string status, string? notes)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"update results 
                                     set FinishTime = @FinishTime, FinishPosition = @FinishPosition, 
                                         TotalParticipants = @TotalParticipants, 
                                         CategoryPosition = @CategoryPosition, 
                                         TotalCategoryParticipants = @TotalCategoryParticipants, 
                                         Status = @Status, Notes = @Notes 
                                     where ResultID = @ResultID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ResultID", result_id);
                        cmd.Parameters.AddWithValue("@FinishTime", finish_time.HasValue ? (object)finish_time.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FinishPosition", finish_position.HasValue ? (object)finish_position.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalParticipants", total_participants.HasValue ? (object)total_participants.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@CategoryPosition", category_position.HasValue ? (object)category_position.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalCategoryParticipants", total_category_participants.HasValue ? (object)total_category_participants.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);

                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Result updated successfully." : "Result not found.";
                    }
                    connect.Close();
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // DELETE
        // ==================================================
        public string delete_result(int result_id)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = "delete from results where ResultID = @ResultID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@ResultID", result_id);
                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Result deleted successfully." : "Result not found.";
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
        private ResultModel map_result(SqlDataReader reader)
        {
            return new ResultModel
            {
                ResultID = Convert.ToInt32(reader["ResultID"]),
                EnrolmentID = Convert.ToInt32(reader["EnrolmentID"]),
                FinishTime = reader["FinishTime"] == DBNull.Value ? null : (TimeSpan)reader["FinishTime"],
                FinishPosition = reader["FinishPosition"] == DBNull.Value ? null : Convert.ToInt32(reader["FinishPosition"]),
                TotalParticipants = reader["TotalParticipants"] == DBNull.Value ? null : Convert.ToInt32(reader["TotalParticipants"]),
                CategoryPosition = reader["CategoryPosition"] == DBNull.Value ? null : Convert.ToInt32(reader["CategoryPosition"]),
                TotalCategoryParticipants = reader["TotalCategoryParticipants"] == DBNull.Value ? null : Convert.ToInt32(reader["TotalCategoryParticipants"]),
                Status = reader["Status"].ToString() ?? "",
                RecordedAt = Convert.ToDateTime(reader["RecordedAt"]),
                RecordedBy = Convert.ToInt32(reader["RecordedBy"]),
                Notes = reader["Notes"] == DBNull.Value ? null : reader["Notes"].ToString()
            };
        }
    }
}