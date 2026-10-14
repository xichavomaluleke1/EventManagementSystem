using System.Data.SqlClient;
using event_management_system.Models;

namespace event_management_system.Database
{//start of namespace

    public class category_dbconnect
    {//start of class

        private readonly string connection_string = @"Data source=(localdb)\api_raceday;Database=raceday_database;Trusted_Connection=True;TrustServerCertificate=True;";

        // STORE a new category
        public string store_category_info(int event_id, string category_name, string? description,
            int? min_age, int? max_age, decimal? min_distance, decimal? max_distance, decimal? entry_fee)
        {//start of store_category_info()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"insert into categories 
                                     (EventID, CategoryName, Description, MinAge, MaxAge, 
                                      MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive) 
                                     values 
                                     (@EventID, @CategoryName, @Description, @MinAge, @MaxAge, 
                                      @MinDistance, @MaxDistance, @EntryFee, GETDATE(), 1);";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EventID", event_id);
                        cmd.Parameters.AddWithValue("@CategoryName", category_name);
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@MinAge",
                            min_age.HasValue ? (object)min_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxAge",
                            max_age.HasValue ? (object)max_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MinDistance",
                            min_distance.HasValue ? (object)min_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxDistance",
                            max_distance.HasValue ? (object)max_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EntryFee",
                            entry_fee.HasValue ? (object)entry_fee.Value : DBNull.Value);

                        cmd.ExecuteNonQuery();

                    }//end of using

                    connect.Close();
                    message = "Category created successfully.";

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of store_category_info()

        // GET all categories 
        public List<Categories> get_all_categories(int? event_id = null)
        {//start of get_all_categories()

            List<Categories> categories = new List<Categories>();

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select CategoryID, EventID, CategoryName, Description, MinAge, MaxAge, 
                                            MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive 
                                     from categories";

                    // if event_id given, filter by it
                    if (event_id.HasValue)
                        query += " where EventID = @EventID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        if (event_id.HasValue)
                            cmd.Parameters.AddWithValue("@EventID", event_id.Value);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            while (reader.Read())
                            {//start of while

                                categories.Add(map_category(reader));

                            }//end of while

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return new List<Categories>();

            }//end of catch

            return categories;

        }//end of get_all_categories()

        // GET categories by event
        public List<Categories> get_categories_by_event(int event_id)
        {//start of get_categories_by_event()

            List<Categories> categories = new List<Categories>();

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select CategoryID, EventID, CategoryName, Description, MinAge, MaxAge, 
                                            MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive 
                                     from categories 
                                     where EventID = @EventID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@EventID", event_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            while (reader.Read())
                            {//start of while

                                categories.Add(map_category(reader));

                            }//end of while

                        }//end of using

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception)
            {//start of catch

                return new List<Categories>();

            }//end of catch

            return categories;

        }//end of get_categories_by_event()

        // GET category by id
        public Categories? get_category_by_id(int category_id)
        {//start of get_category_by_id()

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"select CategoryID, EventID, CategoryName, Description, MinAge, MaxAge, 
                                            MinDistance, MaxDistance, EntryFee, CreatedAt, IsActive 
                                     from categories 
                                     where CategoryID = @CategoryID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@CategoryID", category_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {//start of using

                            if (reader.Read())
                            {//start of if

                                var cat = map_category(reader);
                                connect.Close();
                                return cat;

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

        }//end of get_category_by_id()

        // UPDATE category
        public string update_category_info(int category_id, string category_name, string? description,
            int? min_age, int? max_age, decimal? min_distance, decimal? max_distance,
            decimal? entry_fee, bool? is_active)
        {//start of update_category_info()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = @"update categories 
                                     set CategoryName = @CategoryName, 
                                         Description = @Description, 
                                         MinAge = @MinAge, 
                                         MaxAge = @MaxAge, 
                                         MinDistance = @MinDistance, 
                                         MaxDistance = @MaxDistance, 
                                         EntryFee = @EntryFee, 
                                         IsActive = @IsActive 
                                     where CategoryID = @CategoryID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@CategoryID", category_id);
                        cmd.Parameters.AddWithValue("@CategoryName", category_name);
                        cmd.Parameters.AddWithValue("@Description",
                            string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);
                        cmd.Parameters.AddWithValue("@MinAge",
                            min_age.HasValue ? (object)min_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxAge",
                            max_age.HasValue ? (object)max_age.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MinDistance",
                            min_distance.HasValue ? (object)min_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@MaxDistance",
                            max_distance.HasValue ? (object)max_distance.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EntryFee",
                            entry_fee.HasValue ? (object)entry_fee.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive",
                            is_active.HasValue ? (object)is_active.Value : DBNull.Value);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Category updated successfully.";
                        else
                            message = "Category not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of update_category_info()

        // ==================================================
        // DELETE category
        // ==================================================
        public string delete_category(int category_id)
        {//start of delete_category()

            string message = "";

            try
            {//start of try

                using (SqlConnection connect = new SqlConnection(connection_string))
                {//start of using

                    connect.Open();

                    string query = "delete from categories where CategoryID = @CategoryID";

                    using (SqlCommand cmd = new SqlCommand(query, connect))
                    {//start of using

                        cmd.Parameters.AddWithValue("@CategoryID", category_id);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                            message = "Category deleted successfully.";
                        else
                            message = "Category not found.";

                    }//end of using

                    connect.Close();

                }//end of using

            }//end of try
            catch (Exception error)
            {//start of catch

                message = "Database Error: " + error.Message;

            }//end of catch

            return message;

        }//end of delete_category()

        // ==================================================
        // MAPPER
        // ==================================================
        private Categories map_category(SqlDataReader reader)
        {//start of map_category()

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

        }//end of map_category()

    }//end of class

}//end of namespace