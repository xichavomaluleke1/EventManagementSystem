using System.Data.SqlClient;
using event_management_system.Models;

namespace event_management_system.Database
{//start of namespace

    public class event_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        //STORE a new event
        public string store_event_info(int organiser_id, string event_name, string? description,
            DateTime event_date, string location, decimal distance, string event_type,
            int? max_participants, decimal? entry_fee, string status)
        {//start of store_event_info()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"insert into events 
                                     (OrganiserID, EventName, Description, EventDate, Location, Distance, 
                                      EventType, MaxParticipants, EntryFee, Status, CreatedAt, UpdatedAt, IsPublished) 
                                     values 
                                     (@OrganiserID, @EventName, @Description, @EventDate, @Location, @Distance, 
                                      @EventType, @MaxParticipants, @EntryFee, @Status, GETDATE(), GETDATE(), 1);";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

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

                    }//end of using

                    connect.Close();
                    message = "Event created successfully.";

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of store_event_info()

        //GET all events
        public List<Events> get_all_events()
        {//start of get_all_events()

            List<Events> events = new List<Events>();

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select EventID, OrganiserID, EventName, Description, EventDate, 
                                            Location, Distance, EventType, MaxParticipants, EntryFee, 
                                            BannerImage, Status, CreatedAt, UpdatedAt, IsPublished 
                                     from events 
                                     order by EventDate desc";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            while (reader.Read())
                            {//start of while

                                events.Add(map_event(reader));

                            }//end of while

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return new List<Events>();

            }//end of catch

            return events;

        }//end of get_all_events()

        //GET events by organiser
        public List<Events> get_events_by_organiser(int organiser_id)
        {//start of get_events_by_organiser()

            List<Events> events = new List<Events>();

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select EventID, OrganiserID, EventName, Description, EventDate, 
                                            Location, Distance, EventType, MaxParticipants, EntryFee, 
                                            BannerImage, Status, CreatedAt, UpdatedAt, IsPublished 
                                     from events 
                                     where OrganiserID = @OrganiserID
                                     order by EventDate desc";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@OrganiserID", organiser_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            while (reader.Read())
                            {//start of while

                                events.Add(map_event(reader));

                            }//end of while

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return new List<Events>();

            }//end of catch

            return events;

        }//end of get_events_by_organiser()

        //GET event by id
        public Events? get_event_by_id(int event_id)
        {//start of get_event_by_id()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select EventID, OrganiserID, EventName, Description, EventDate, 
                                            Location, Distance, EventType, MaxParticipants, EntryFee, 
                                            BannerImage, Status, CreatedAt, UpdatedAt, IsPublished 
                                     from events 
                                     where EventID = @EventID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EventID", event_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {//start of if

                                var ev = map_event(reader);
                                connect.Close();
                                return ev;

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

        }//end of get_event_by_id()

        //UPDATE event
        public string update_event_info(int event_id, string event_name, string? description,
            DateTime event_date, string location, decimal distance, string event_type,
            int? max_participants, decimal? entry_fee, string status)
        {//start of update_event_info()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update events 
                                     set EventName = @EventName, 
                                         Description = @Description, 
                                         EventDate = @EventDate, 
                                         Location = @Location, 
                                         Distance = @Distance, 
                                         EventType = @EventType, 
                                         MaxParticipants = @MaxParticipants, 
                                         EntryFee = @EntryFee, 
                                         Status = @Status, 
                                         UpdatedAt = GETDATE() 
                                     where EventID = @EventID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

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

                        if (rows > 0)
                            message = "Event updated successfully.";
                        else
                            message = "Event not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of update_event_info()

        //DELETE event
        public string delete_event(int event_id)
        {//start of delete_event()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = "delete from events where EventID = @EventID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EventID", event_id);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Event deleted successfully.";
                        else
                            message = "Event not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of delete_event()

        //PUBLISH / UNPUBLISH event
        public string publish_event(int event_id, bool is_published)
        {//start of publish_event()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update events 
                                     set IsPublished = @IsPublished, 
                                         UpdatedAt = GETDATE() 
                                     where EventID = @EventID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        cmd.Parameters.AddWithValue("@IsPublished", is_published ? 1 : 0);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Event publish status updated.";
                        else
                            message = "Event not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of publish_event()

        //MAPPER
        private Events map_event(SqlDataReader reader)
        {//start of map_event()

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

        }//end of map_event()

    }//end of class

}//end of namespace