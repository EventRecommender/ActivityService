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
            //"INSERT INTO `table` (`value1`, `value2`) " +
            //"SELECT 'stuff for value1', 'stuff for value2' FROM DUAL " +
            //"WHERE NOT EXISTS (SELECT * FROM `table` " +
            //  "WHERE `value1`='stuff for value1' AND `value2`='stuff for value2' LIMIT 1)"
            string query = $"INSERT INTO activity (title, host, place, time, img, path, description, active) " +
                        $"SELECT '{activity.title}', '{activity.host}', '{activity.place}', '{activity.date}', '{activity.img}', '{activity.url}', '{activity.description}', {activity.active} FROM DUAL " +
                        $"WHERE NOT EXISTS (SELECT * FROM activity " +
                            $"WHERE title = '{activity.title}' AND host = '{activity.host}' AND place = '{activity.place}' AND time = '{activity.date}' AND img = '{activity.img}' AND path = '{activity.url}' AND description = '{activity.description}' AND active = {activity.active} LIMIT 1)";

            //string query = $"IF NOT EXISTS (SELECT * FROM 'activity' " +
            //                                $"WHERE title = '{activity.title}' " +
            //                                $"AND host = '{activity.host}' " +
            //                                $"AND place = '{activity.location}' " +
            //                                $"AND time = {activity.date} " +
            //                                $"AND img = '{activity.imageurl}' " +
            //                                $"AND path = '{activity.url}' " +
            //                                $"AND description = '{activity.description}' " +
            //                                $"AND active = {activity.active})" +
            //                $"BEGIN" +
            //                    $"INSERT INTO activity (title, host, place, time, img, path, description, active) " +
            //                    $"VALUES ('{activity.title}', '{activity.host}', '{activity.location}', {activity.date}, '{activity.imageurl}', '{activity.url}', '{activity.description}', {activity.active});" +
            //                $"END;";

            //var sb = new System.Text.StringBuilder();
            //foreach (Activity x in activityList.Skip(1))
            //{
            //    sb.AppendLine($", ('{x.title}', '{x.host}', '{x.location}', {x.date}, '{x.imageurl}', '{x.url}', '{x.description}', {x.active})");
            //}
            //query += sb + ";";


            using MySqlConnection con = new MySqlConnection(connectionString);
            MySqlCommand command = new MySqlCommand(query, con);

            try
            {
                con.Open();
                MySqlDataReader reader = command.ExecuteReader();
                reader.Close();
                command.Dispose();
                con.Close();
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
                con.Close();
                Console.Write("[DATABASE][/AddActivity] InvalidOperationException. Failed to add activity to database: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
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
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), new List<string>(), reader.GetBoolean(8))); 
                }

                reader.Close();
                command.Dispose();

				//Add type
				foreach (Activity activity in activities)
				{
					query = $"SELECT * FROM type WHERE activityid = {activity.id}";
					using var command2 = new MySqlCommand(query, con);
					using MySqlDataReader reader2 = command2.ExecuteReader();
					while (reader2.Read())
					{
						activity.type.Add(reader.GetString(1));
					}
					command2.Dispose();
				}


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
            DateTime specifiedDate = (DateTime.Now).AddMonths(monthsForward);
            var query = $"SELECT * FROM activity WHERE place='{city}' AND time BETWEEN '{currentDate}' AND '{specifiedDate}'";
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
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), new List<string>(), reader.GetBoolean(8)));
                }

                reader.Close();
                command.Dispose();


				//Add type
				foreach (Activity activity in activities)
				{
					query = $"SELECT * FROM type WHERE activityid = {activity.id}";
					using var command2 = new MySqlCommand(query, con);
					using MySqlDataReader reader2 = command2.ExecuteReader();
					while (reader2.Read())
					{
						activity.type.Add(reader.GetString(1));
					}
					command2.Dispose();
				}





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
            string query = $"SELECT * FROM activity WHERE id IN ('{listOfActivityID.First()}'";

            var sb = new System.Text.StringBuilder();
            foreach (int x in listOfActivityID.Skip(1)){
                sb.AppendLine($", '{x}'");
            }
            query += sb + ");";

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
                                        reader.GetString(5), reader.GetString(6), reader.GetString(7), new List<string>(), reader.GetBoolean(8)));
                }

                reader.Close();
                command.Dispose();
                
                

                //Add type
				foreach (Activity activity in activities)
				{
					query = $"SELECT * FROM type WHERE activityid = {activity.id}";
					using var command2 = new MySqlCommand(query, con);
					using MySqlDataReader reader2 = command2.ExecuteReader();
					while (reader2.Read())
					{
						activity.type.Add(reader.GetString(1));
					}
                    command2.Dispose();
				}

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
            // different way to do what you did.
            // i dont know which is better but this does not use a stringbuilder which might make a difference - Mads
       
            List<string> rows = new();
            foreach (string pref in listOfPreferences)
            {
                rows.Add("'" + pref + "'");
            }
            string prefList = string.Join(",",rows);


            var query = $"SELECT activityid FROM type WHERE tag IN ({prefList})";
            /*
            var sb = new System.Text.StringBuilder();
            foreach (string x in listOfPreferences.Skip(1))
            {
                sb.AppendLine($", {x}");
            }
            query += sb + ");";
            */

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
            var query = $"SELECT activityid FROM type WHERE host = {organizerId}";
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
                                            reader.GetString(5), reader.GetString(6), reader.GetString(7), new List<string>(), reader.GetBoolean(8)));
                }

                reader.Close();
                command.Dispose();

				//Add type
				foreach (Activity activity in activities)
				{
					query = $"SELECT * FROM type WHERE activityid = {activity.id}";
					using MySqlDataReader reader2 = command.ExecuteReader();
					while (reader2.Read())
					{
						activity.type.Add(reader.GetString(1));
					}
				}


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
                var json = await client.GetStringAsync("http://scraper:5000/Scrape");
                //throw new Exception(json);
                if (json != null)
                {
                    var activitiesToAdd = JsonSerializer.Deserialize<List<List<Activity>>>(json)!;
                    foreach (List<Activity> list in activitiesToAdd)
                    {
                        foreach(Activity a in list) 
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
            catch (Exception e)
            {
                Console.Write("############################################################################################################################# " + e.Message);
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

			MySqlDataAdapter adapter = new MySqlDataAdapter();

			try
            {
                con.Open();

				

				adapter.InsertCommand = command;
				adapter.InsertCommand.ExecuteNonQuery();

                command.Dispose();
                adapter.Dispose();
                con.Close();
            }
            catch (InvalidOperationException e)
            {
                command.Dispose();
				adapter.Dispose();
				con.Close();
                Console.Write("[DATABASE][/RemoveActivities] InvalidOperationException. Failed to remove activities: \n" + e.Message);
                throw;
            }
            catch (MySqlException e)
            {
                command.Dispose();
				adapter.Dispose();
				con.Close();
                Console.Write("[DATABASE][/RemoveActivities] MySqlException. Failed to remove activities: \n" + e.Message);
                throw;
            }
        }
    }
}
