using System;
using System.Reflection.PortableExecutable;
using MySql.Data.MySqlClient;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ActivityService.Classes
{
    public class Database
    {
        public Database()
        {

        }

        string connectionString = @"server=activity_database;userid=root;password=super;database=activity_db";

        //Adds an activity to the database
        public void AddActivity(Activity activity)
        {
            string query = $"INSERT INTO activity (title, host, place, time, img, path, description, active) " +
                           $"SELECT '{activity.title}' AS title, '{activity.host}' AS host, '{activity.place}' AS place, '{activity.date}' AS time, '{activity.img}' AS img, '{activity.url}' AS path, '{activity.description}' AS description, {activity.active} AS active FROM DUAL " +
                           $"WHERE NOT EXISTS (SELECT * FROM activity " +
                           $"WHERE 'title' = '{activity.title}' AND host = '{activity.host}' AND place = '{activity.place}' AND time = '{activity.date}' AND img = '{activity.img}' AND path = '{activity.url}' AND description = '{activity.description}' AND active = {activity.active} LIMIT 1)";

            string typeQuery = $"INSERT INTO type(activityid, tag) " +
                               $"VALUES((SELECT activity.id FROM activity WHERE activity.title = '{activity.title}' AND host = '{activity.host}' AND place = '{activity.place}' AND time = '{activity.date}' AND description = '{activity.description}' LIMIT 1), '{activity.type}');";

            using MySqlConnection con = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, con);
            MySqlCommand typeCommand = new MySqlCommand(typeQuery, con);

            try
            {
                con.Open();
                var tr = con.BeginTransaction();
                command.ExecuteNonQuery();
                typeCommand.ExecuteNonQuery();
                command.Dispose();
                typeCommand.Dispose();
                tr.Commit();
                con.Close();
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                typeCommand.Dispose();

                con.Close();
                Console.Write("[DATABASE][/AddActivity] InvalidOperationException. Failed to add activity to database: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                typeCommand.Dispose();

                con.Close();
                Console.Write("[DATABASE][/AddActivity] MySqlException. Failed to add activity to database: \n" + e.Message);
                throw;
            }
        }
        
        //Retrieves all activities hosted in a specific city
        public List<Activity> GetActivities(string city)
        {
            var query = $"SELECT * FROM activity WHERE place='{city}';";
            List<Activity> activities = new List<Activity>();

            using var con = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, con);

            try
            {
                con.Open();
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();
                con.Close();
                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        //Retrieves all activities hosted in a specific city within a given time period
        public List<Activity> GetActivities(string city, int monthsForward)
        {
            DateTime currentDate = DateTime.Now;
            DateTime specifiedDate = DateTime.Now.AddMonths(monthsForward);
            var query = $"SELECT * FROM activity WHERE place = '{city}' AND time BETWEEN current_timestamp() AND '{specifiedDate.ToString("yyyy-MM-dd HH:mm:ss")}';";
            List<Activity> activities = new List<Activity>();

            using var con = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, con);

            try
            {
                con.Open();
                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();
                con.Close();
                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        //Retrieves specific activities by given ID's
        public List<Activity> GetActivities(List<int> listOfActivityID)
        {
            string pref = "";
            foreach (int i in listOfActivityID)
            {
                pref += "'" + i.ToString() + "',";
            }
            pref = pref.Remove(pref.Length -1,1);

            string query = $"SELECT * FROM activity WHERE id IN ({pref})";

            var sb = new System.Text.StringBuilder();

            using var con = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, con);

            try
            {
                con.Open();
                using MySqlDataReader reader = command.ExecuteReader();

                List<Activity> activities = new List<Activity>();
                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();
                con.Close();
                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        //Retrieves activities containing specific tags
        public List<Activity> GetActivitiesByPreference(List<string> listOfPreferences)
        {
            string pref = "";
            foreach (string s in listOfPreferences)
            {
                pref += "'" + s + "',";
            }
            pref = pref.Remove(pref.Length -1,1);

            var query = $"SELECT activityid FROM type WHERE tag IN ({pref})";
            var sb = new System.Text.StringBuilder();

            using var con = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, con);

            try
            {
                con.Open();

                using MySqlDataReader reader = command.ExecuteReader();

                List<int> activityList = new List<int>();
                while (reader.Read())
                {
                    activityList.Add(reader.GetInt32(0));
                }

                reader.Close();
                command.Dispose();
                con.Close();
                return GetActivities(activityList);
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivitiesByPreference] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetActivitiesByPreference] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        //Retrieves all activities created by a specific organization
        public List<Activity> GetUserActivities(int organizerId)
        {
            var query = $"SELECT * FROM activity WHERE host = {organizerId}";
            List<Activity> activities = new List<Activity>();

            using var con = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, con);

            try
            {
                con.Open();

                using MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    activities.Add(new Activity(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4),
                                            reader.GetString(5), reader.GetString(6), reader.GetString(7), reader.GetBoolean(8), ""));
                }

                reader.Close();
                command.Dispose();
                con.Close();
                return activities;
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetUserActivities] InvalidOperationException. Failed to get activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/GetUserActivities] MySqlException. Failed to get activities: \n" + e.Message);
                throw;
            }
        }

        //Starts retrieval of new activities, using API calls
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

        //Removes activities from the database
        public void RemoveActivities(List<int> listOfActivityID) 
        {
            var query = $"DELETE FROM activity WHERE id IN ('{listOfActivityID.First()}'";

            var sb = new System.Text.StringBuilder();
            foreach (int x in listOfActivityID.Skip(1))
            {
                sb.AppendLine($", '{x}'");
            }
            query += sb + ");";

            using var con = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, con);

            try
            {
                con.Open();

                using MySqlDataReader reader = command.ExecuteReader();

                reader.Close();
                command.Dispose();
                con.Close();
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/RemoveActivities] InvalidOperationException. Failed to remove activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/RemoveActivities] MySqlException. Failed to remove activities: \n" + e.Message);
                throw;
            }
        }
    }
}
