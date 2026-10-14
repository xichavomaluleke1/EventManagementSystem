using System.Data.SqlClient;
using event_management_system.Models;
using event_management_system.Database.Interfaces;

namespace event_management_system.Database
{//start of namespace

    public class event_dbconnect : Ievent_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // ==================================================
        // STORE a new event
        // ==================================================
        public string store_event_info(int organiser_id, string event_name, string? description,
            DateTime event_date, string location, decimal distance, string event_type,
            int? max_participants, decimal? entry_fee, string status)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"insert into events 
                                     (OrganiserID, EventName, Description, EventDate, Location, Distance, 
                                      EventType, MaxParticipants, EntryFee, Status, CreatedAt, UpdatedAt, IsPublished) 
                                     values 
                                     (@OrganiserID, @EventName, @Description, @EventDate, @Location, @Distance, 
                                      @EventType, @MaxParticipants, @EntryFee, @Status, GETDATE(), GETDATE(), 1);";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@OrganiserID", organiser_id);
                        cmd.Parameters.AddWithValue("@EventName", event_name);
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@EventDate", event_date);
                        cmd.Parameters.AddWithValue("@Location", location);
                        cmd.Parameters.AddWithValue("@Distance", distance);
                        cmd.Parameters.AddWithValue("@EventType", event_type);
                        cmd.Parameters.AddWithValue("@MaxParticipants",
                            max_participants.HasValue ? (object)max_participants.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EntryFee",
                            entry_fee.HasValue ? (object)entry_fee.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", status);

                        cmd.ExecuteNonQuery();
                    }
                    connect.Close();
                    message = "Event created successfully.";
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // GET all events
        // ==================================================
        public List<Events> get_all_events()
        {
            List<Events> events = new List<Events>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select EventID, OrganiserID, EventName, Description, EventDate, 
                                            Location, Distance, EventType, MaxParticipants, EntryFee, 
                                            BannerImage, Status, CreatedAt, UpdatedAt, IsPublished 
                                     from events order by EventDate desc";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) events.Add(map_event(reader));
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<Events>(); }
            return events;
        }

        // ==================================================
        // GET events by organiser
        // ==================================================
        public List<Events> get_events_by_organiser(int organiser_id)
        {
            List<Events> events = new List<Events>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select EventID, OrganiserID, EventName, Description, EventDate, 
                                            Location, Distance, EventType, MaxParticipants, EntryFee, 
                                            BannerImage, Status, CreatedAt, UpdatedAt, IsPublished 
                                     from events where OrganiserID = @OrganiserID 
                                     order by EventDate desc";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@OrganiserID", organiser_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) events.Add(map_event(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<Events>(); }
            return events;
        }

        // ==================================================
        // GET event by id
        // ==================================================
        public Events? get_event_by_id(int event_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select EventID, OrganiserID, EventName, Description, EventDate, 
                                            Location, Distance, EventType, MaxParticipants, EntryFee, 
                                            BannerImage, Status, CreatedAt, UpdatedAt, IsPublished 
                                     from events where EventID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var ev = map_event(reader);
                                connect.Close();
                                return ev;
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
        // UPDATE event
        // ==================================================
        public string update_event_info(int event_id, string event_name, string? description,
            DateTime event_date, string location, decimal distance, string event_type,
            int? max_participants, decimal? entry_fee, string status)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"update events 
                                     set EventName = @EventName, Description = @Description, 
                                         EventDate = @EventDate, Location = @Location, 
                                         Distance = @Distance, EventType = @EventType, 
                                         MaxParticipants = @MaxParticipants, EntryFee = @EntryFee, 
                                         Status = @Status, UpdatedAt = GETDATE() 
                                     where EventID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        cmd.Parameters.AddWithValue("@EventName", event_name);
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@EventDate", event_date);
                        cmd.Parameters.AddWithValue("@Location", location);
                        cmd.Parameters.AddWithValue("@Distance", distance);
                        cmd.Parameters.AddWithValue("@EventType", event_type);
                        cmd.Parameters.AddWithValue("@MaxParticipants",
                            max_participants.HasValue ? (object)max_participants.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EntryFee",
                            entry_fee.HasValue ? (object)entry_fee.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@Status", status);

                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Event updated successfully." : "Event not found.";
                    }
                    connect.Close();
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // DELETE event
        // ==================================================
        public string delete_event(int event_id)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = "delete from events where EventID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Event deleted successfully." : "Event not found.";
                    }
                    connect.Close();
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // PUBLISH / UNPUBLISH event
        // ==================================================
        public string publish_event(int event_id, bool is_published)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"update events 
                                     set IsPublished = @IsPublished, UpdatedAt = GETDATE() 
                                     where EventID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        cmd.Parameters.AddWithValue("@IsPublished", is_published ? 1 : 0);
                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Event publish status updated." : "Event not found.";
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
        private Events map_event(SqlDataReader reader)
        {
            return new Events
            {
                EventID = Convert.ToInt32(reader["EventID"]),
                OrganiserID = Convert.ToInt32(reader["OrganiserID"]),
                EventName = reader["EventName"].ToString() ?? "",
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                EventDate = Convert.ToDateTime(reader["EventDate"]),
                Location = reader["Location"].ToString() ?? "",
                Distance = Convert.ToDecimal(reader["Distance"]),
                EventType = reader["EventType"].ToString() ?? "",
                MaxParticipants = reader["MaxParticipants"] == DBNull.Value ? null : Convert.ToInt32(reader["MaxParticipants"]),
                EntryFee = reader["EntryFee"] == DBNull.Value ? null : Convert.ToDecimal(reader["EntryFee"]),
                BannerImage = reader["BannerImage"] == DBNull.Value ? null : reader["BannerImage"].ToString(),
                Status = reader["Status"].ToString() ?? "",
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["UpdatedAt"]),
                IsPublished = reader["IsPublished"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsPublished"])
            };
        }
    }
}