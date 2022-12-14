using System;
using System.Reflection.PortableExecutable;
using MySql.Data.MySqlClient;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Data;

namespace ActivityService.Classes
{
    public class DatabaseHandler
    {
        string connectionString { get; set; }
        public DatabaseHandler()
        {
            connectionString = @"server=activity_database;userid=root;password=super;database=activity_db";
        }

        public DatabaseHandler(string _connectionString)
        {
            connectionString = _connectionString;
        }

        private List<Activity> GetTypes(List<Activity> activities, MySqlConnection connection)
        {
            //Add type
            foreach (Activity activity in activities)
            {
                string query = $"SELECT * FROM type WHERE activityid = {activity.id}";
                using var command2 = new MySqlCommand(query, connection);
                using MySqlDataReader reader2 = command2.ExecuteReader();

                if (reader2.Read())
                {
                    activity.type = reader2.GetString(1);
                }

                reader2.Close();
                command2.Dispose();
            }

            return activities;
        }


        /// <summary>
        /// Adds an activity and its type tags to the database.
        /// </summary>
        /// <param name="activity">An object of the Activity class which is to be added to the database.</param>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="MySqlException">Thrown whenever MySQL returns an error.</exception>
        public void AddActivity(Activity activity)
        {
            string query = GenerateAddActivityQuery(activity);
            string typeQuery = GenerateAddTypeQuery(activity);

            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);
            MySqlCommand typeCommand = new MySqlCommand(typeQuery, connection);

            try
            {
                connection.Open();

                var tr = connection.BeginTransaction();
                command.ExecuteNonQuery();
                typeCommand.ExecuteNonQuery();
                command.Dispose();
                typeCommand.Dispose();

                tr.Commit();
                connection.Close();
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                typeCommand.Dispose();

                connection.Close();
                Console.Write("[DATABASE][/AddActivity] InvalidOperationException. Failed to add activity to database: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                typeCommand.Dispose();

                connection.Close();
                Console.Write("[DATABASE][/AddActivity] MySqlException. Failed to add activity to database: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all activities hosted in a specific city
        /// </summary>
        /// <param name="city">A string of the city from where activities should be retrieved.</param>
        /// <returns>A list of all activities that are held in the city given as input.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="MySqlException">Thrown whenever MySQL returns an error.</exception>
        public List<Activity> GetActivities(string city)
        {
            var query = $"SELECT * FROM activity WHERE place='{city.ToLower()}';";
            List<Activity> activities = new List<Activity>();

            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();

                activities = GetTypes(activities, connection);

                connection.Close();

                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all activities hosted in a specific city within a given time period.
        /// </summary>
        /// <param name="city">A string of the city from where activities should be retrieved.</param>
        /// <param name="monthsForward">An integer telling how many months forward activities should be retrieved.</param>
        /// <returns>A list of all activities that are held in the city given as input, and are held within a given time frame.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="MySqlException">Thrown whenever MySQL returns an error.</exception>
        public List<Activity> GetActivities(string city, int monthsForward)
        {
            DateTime currentDate = DateTime.Now;
            DateTime specifiedDate = DateTime.Now.AddMonths(monthsForward);
            var query = $"SELECT * FROM activity WHERE place = '{city.ToLower()}' AND time BETWEEN current_timestamp() AND '{specifiedDate.ToString("yyyy-MM-dd HH:mm:ss")}';";
            List<Activity> activities = new List<Activity>();

            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();

                activities = GetTypes(activities, connection);

                connection.Close();
                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves specific activities by given ID's.
        /// </summary>
        /// <param name="listOfActivityID">An integer list of activity ID's.</param>
        /// <returns>A list of all activities that matches with the id's given as input.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="MySqlException">Thrown whenever MySQL returns an error.</exception>
        public List<Activity> GetActivities(List<int> listOfActivityID)
        {
            if (listOfActivityID.Count == 0)
            {
                return new List<Activity>();
            }
            string pref = "";
            foreach (int i in listOfActivityID)
            {
                pref += "'" + i.ToString() + "',";
            }
            pref = pref.Remove(pref.Length -1,1);

            string query = $"SELECT * FROM activity WHERE id IN ({pref})";

            var sb = new System.Text.StringBuilder();

            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            try
            {
                connection.Open();
                using MySqlDataReader reader = command.ExecuteReader();

                List<Activity> activities = new List<Activity>();
                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();

                activities = GetTypes(activities, connection);

                connection.Close();
                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves activities containing specific tags.
        /// </summary>
        /// <param name="listOfPreferences">A string list of the users preferences.</param>
        /// <returns>A list of all activities that contain at least one of the tags given as input.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="MySqlException">Thrown whenever MySQL returns an error.</exception>
        public List<Activity> GetActivitiesByPreference(Dictionary<string, int> preferenceDictionary)
        {
            
            //Create SQL string for inserting into db
            StringBuilder sb = new StringBuilder();
            List<string> queryList = new List<string>();
            if (preferenceDictionary.Count != 0)
            {
                foreach (KeyValuePair<string, int> s in preferenceDictionary)
                {
                    queryList.Add($"(SELECT activityid FROM type WHERE tag = '{s.Key}' LIMIT {s.Value})");
                }
                sb.Append(string.Join(" UNION ", queryList));
                sb.Append(";");
            }
            else
            {
                return new List<Activity>();
            }
            string query = sb.ToString();

            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);
            
            try
            {
                connection.Open();

                using MySqlDataReader reader = command.ExecuteReader();

                List<int> activityList = new List<int>();
                while (reader.Read())
                {
                    activityList.Add(reader.GetInt32(0));
                }

                reader.Close();
                command.Dispose();
                connection.Close();
                return GetActivities(activityList);
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivitiesByPreference] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetActivitiesByPreference] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves all activities created by a specific organization.
        /// </summary>
        /// <param name="organizerId">An integer containing the user ID of the organization.</param>
        /// <returns>A list of all activities that are created by the given user.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="MySqlException">Thrown whenever MySQL returns an error.</exception>
        public List<Activity> GetUserActivities(string username)
        {
            var query = $"SELECT * FROM activity WHERE host = '{username.ToLower()}'";
            List<Activity> activities = new List<Activity>();

            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            try
            {
                connection.Open();

                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                            reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();

                activities = GetTypes(activities, connection);

                connection.Close();

                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetUserActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/GetUserActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Starts retrieval of new activities, using an API call to the event scraper.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="HttpRequestException">Thrown when an error occurs when trying to send http requests.</exception>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="ArgumentNullException">Thrown when an invalid URL is given to GetStringAsync().</exception>
        public async void UpdateActivities()
        {
            try
            {
                using HttpClient client = new();
                var json = await client.GetStringAsync("http://scraper-service:5000/Scrape");
                if (json != null)
                {
                    var activitiesToAdd = JsonSerializer.Deserialize<List<List<Activity>>>(json)!;
                    foreach (List<Activity> list in activitiesToAdd)
                    {
                        foreach (Activity a in list)
                        {
                            AddActivity(a);
                        }
                    }
                        
                }
                else
                    throw new ArgumentNullException();
            }
            catch (InvalidOperationException e)
            {
                Console.Write("[DATABASE][/UpdateActivities] InvalidOperationException. Failed to update activities: \n" + e.Message);
                throw;
            }
            catch (HttpRequestException e)
            {
                Console.Write("[DATABASE][/UpdateActivities] HttpRequestException. Failed to connect to Scraper: \n" + e.Message);
                throw;
            }
            catch (TaskCanceledException e)
            {
                Console.Write("[DATABASE][/UpdateActivities] TaskCanceledException. Task cancelled: \n" + e.Message);
                throw;
            }
            catch (ArgumentNullException e)
            {
                Console.Write("[DATABASE][/UpdateActivities] ArgumentNullException. No activities fetched: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Removes activities from the database.
        /// </summary>
        /// <param name="listOfActivityID">An integer list of activity ID's.</param>
        /// <exception cref="InvalidOperationException">Thrown when a method call is invalid for the object's current state.</exception>
        /// <exception cref="MySqlException">Thrown whenever MySQL returns an error.</exception>
        public void RemoveActivities(List<int> listOfActivityID) 
        {
            var query = $"DELETE FROM activity WHERE id IN ('{listOfActivityID.First()}'";

            var sb = new System.Text.StringBuilder();
            foreach (int x in listOfActivityID.Skip(1))
            {
                sb.AppendLine($", '{x}'");
            }
            query += sb + ");";

            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, connection);

            try
            {
                connection.Open();

                using MySqlDataReader reader = command.ExecuteReader();

                reader.Close();
                command.Dispose();
                connection.Close();
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/RemoveActivities] InvalidOperationException. Failed to remove activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                connection.Close();
                Console.Write("[DATABASE][/RemoveActivities] MySqlException. Failed to remove activities: \n" + e.Message);
                throw;
            }
        }

        /// <summary>
        /// Generates a query which is used to add an activity to the database.
        /// </summary>
        /// <param name="activity">An object of the Activity class which is used to generate the query.</param>
        /// <returns>A string containing a MySQL query for adding activities to the Activity table.</returns>
        private string GenerateAddActivityQuery(Activity activity)
        {
            string query =  $"INSERT INTO activity (title, host, place, time, img, path, description, active) " +
                            $"SELECT '{activity.title.ToLower()}' AS title, " +
                                    $"'{activity.host.ToLower()}' AS host, " +
                                    $"'{activity.place.ToLower()}' AS place, " +
                                    $"'{activity.date}' AS time, " +
                                    $"'{activity.img.ToLower()}' AS img, " +
                                    $"'{activity.url.ToLower()}' AS path, " +
                                    $"'{activity.description.ToLower()}' AS description, " +
                                    $"{activity.active} AS active FROM DUAL " +
                            $"WHERE NOT EXISTS (SELECT * FROM activity " +
                            $"WHERE title = '{activity.title.ToLower()}' AND " +
                                    $"host = '{activity.host.ToLower()}' AND " +
                                    $"place = '{activity.place.ToLower()}' AND " +
                                    $"time = '{activity.date}' AND " +
                                    $"img = '{activity.img.ToLower()}' AND " +
                                    $"path = '{activity.url.ToLower()}' AND " +
                                    $"description = '{activity.description.ToLower()}' AND " +
                                    $"active = {activity.active} LIMIT 1)";

            return query;
        }

        /// <summary>
        /// Generates a query which is used to assign a type to an activity on the database.
        /// </summary>
        /// <param name="activity">An object of the Activity class which is used to generate the query.</param>
        /// <returns>A string containing a MySQL query for assigning types to activities on the Type table.</returns>
        private string GenerateAddTypeQuery(Activity activity)
        {
            string query = $"INSERT INTO type(activityid, tag) " +
                           $"VALUES((SELECT activity.id FROM activity " +
                               $"WHERE title = '{activity.title.ToLower()}' AND " +
                                        $"host = '{activity.host.ToLower()}' AND " +
                                        $"place = '{activity.place.ToLower()}' AND " +
                                        $"time = '{activity.date}' AND " +
                                        $"description = '{activity.description.ToLower()}' LIMIT 1), " +
                                $"'{activity.type.ToLower()}');";

            return query;
        }

        /// <summary>
        /// ONLY USE FOR TESTING. DO NOT USE IN PRODUCTION
        /// Clears the entire database.
        /// </summary>
        public void ClearDatabase()
        {
            MySqlConnection connection = new(connectionString);
            connection.Open();
            string SQLstatement = "DELETE FROM activity;";

            //Execute SQL
            MySqlCommand command = new MySqlCommand(SQLstatement, connection);
            MySqlDataAdapter adapter = new MySqlDataAdapter();


            command.CommandType = CommandType.Text;
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();

            command.Dispose();
            adapter.Dispose();

            connection.Close();
        }
    }
}
