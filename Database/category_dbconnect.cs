using System.Data.SqlClient;
using event_management_system.Models;
using event_management_system.Database.Interfaces;

namespace event_management_system.Database
{//start of namespace

    public class category_dbconnect : Icategory_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // ==================================================
        // STORE a new category
        // ==================================================
        public string store_category_info(int event_id, string category_name, string? description,
            int? min_age, int? max_age, decimal? min_distance, decimal? max_distance, decimal? entry_fee)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"insert into categories 
                                     (EventID, CategoryName, Description, MinAge, MaxAge, 
                                      MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive) 
                                     values 
                                     (@EventID, @CategoryName, @Description, @MinAge, @MaxAge, 
                                      @MinDistance, @MaxDistance, @EntryFee, GETDATE(), 1);";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        cmd.Parameters.AddWithValue("@CategoryName", category_name);
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@MinAge", min_age.HasValue ? (object)min_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxAge", max_age.HasValue ? (object)max_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MinDistance", min_distance.HasValue ? (object)min_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxDistance", max_distance.HasValue ? (object)max_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EntryFee", entry_fee.HasValue ? (object)entry_fee.Value : DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    connect.Close();
                    message = "Category created successfully.";
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // GET all categories (optionally filtered by event)
        // ==================================================
        public List<Categories> get_all_categories(int? event_id = null)
        {
            List<Categories> categories = new List<Categories>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select CategoryID, EventID, CategoryName, Description, MinAge, MaxAge, 
                                            MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive 
                                     from categories";
                    if (event_id.HasValue) query += " where EventID = @EventID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        if (event_id.HasValue) cmd.Parameters.AddWithValue("@EventID", event_id.Value);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) categories.Add(map_category(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<Categories>(); }
            return categories;
        }

        // ==================================================
        // GET categories by event
        // ==================================================
        public List<Categories> get_categories_by_event(int event_id)
        {
            List<Categories> categories = new List<Categories>();
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select CategoryID, EventID, CategoryName, Description, MinAge, MaxAge, 
                                            MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive 
                                     from categories where EventID = @EventID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) categories.Add(map_category(reader));
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception) { return new List<Categories>(); }
            return categories;
        }

        // ==================================================
        // GET category by id
        // ==================================================
        public Categories? get_category_by_id(int category_id)
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"select CategoryID, EventID, CategoryName, Description, MinAge, MaxAge, 
                                            MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive 
                                     from categories where CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", category_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var cat = map_category(reader);
                                connect.Close();
                                return cat;
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
        // UPDATE category
        // ==================================================
        public string update_category_info(int category_id, string category_name, string? description,
            int? min_age, int? max_age, decimal? min_distance, decimal? max_distance,
            decimal? entry_fee, bool? is_active)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = @"update categories 
                                     set CategoryName = @CategoryName, Description = @Description, 
                                         MinAge = @MinAge, MaxAge = @MaxAge, 
                                         MinDistance = @MinDistance, MaxDistance = @MaxDistance, 
                                         EntryFee = @EntryFee, IsActive = @IsActive 
                                     where CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", category_id);
                        cmd.Parameters.AddWithValue("@CategoryName", category_name);
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@MinAge", min_age.HasValue ? (object)min_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxAge", max_age.HasValue ? (object)max_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MinDistance", min_distance.HasValue ? (object)min_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxDistance", max_distance.HasValue ? (object)max_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EntryFee", entry_fee.HasValue ? (object)entry_fee.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", is_active.HasValue ? (object)is_active.Value : DBNull.Value);

                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Category updated successfully." : "Category not found.";
                    }
                    connect.Close();
                }
            }
            catch (Exception error) { message = "Database Error: " + error.Message; }
            return message;
        }

        // ==================================================
        // DELETE category
        // ==================================================
        public string delete_category(int category_id)
        {
            string message = "";
            try
            {
                using (SqlConnection connect = new SqlConnection(connection_string))
                {
                    connect.Open();
                    string query = "delete from categories where CategoryID = @CategoryID";
                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@CategoryID", category_id);
                        int rows = cmd.ExecuteNonQuery();
                        message = rows > 0 ? "Category deleted successfully." : "Category not found.";
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
        private Categories map_category(SqlDataReader reader)
        {
            return new Categories
            {
                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                EventID = Convert.ToInt32(reader["EventID"]),
                CategoryName = reader["CategoryName"].ToString() ?? "",
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                MinAge = reader["MinAge"] == DBNull.Value ? null : Convert.ToInt32(reader["MinAge"]),
                MaxAge = reader["MaxAge"] == DBNull.Value ? null : Convert.ToInt32(reader["MaxAge"]),
                MinDistance = reader["MinDistance"] == DBNull.Value ? null : Convert.ToDecimal(reader["MinDistance"]),
                MaxDistance = reader["MaxDistance"] == DBNull.Value ? null : Convert.ToDecimal(reader["MaxDistance"]),
                EntryFee = reader["EntryFee"] == DBNull.Value ? null : Convert.ToDecimal(reader["EntryFee"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                IsActive = reader["IsActive"] == DBNull.Value ? false : Convert.ToBoolean(reader["IsActive"])
            };
        }
    }
}