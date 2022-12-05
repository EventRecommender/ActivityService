using System;
using System.Data.SqlClient;
using Npgsql;

namespace ActivityService.Classes
{
    public class Database
    {
        public Database()
        {

        }

        //The string used to connect to the SQL database
        string connectionString = "server=localhost:3306;uid=root;pwd=super;database=test";

        //Adds an activity to the database
        public void AddActivity(Activity activity)
        {
            string query = $"INSERT INTO activity (title, host, location, date, imageurl, url, description, active) VALUES " +
                $"({activity.title}, {activity.host}, {activity.location}, {activity.date}, {activity.imageurl}, {activity.url}, {activity.description}, {activity.active});";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(query, con);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();
            command.Dispose();
            con.Close();
        }
        
        //Retrieves all activities hosted in a specific city
        public List<Activity> GetActivities(string city)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            var query = $"SELECT * FROM activity WHERE location='{city}';";

            using var cmd = new NpgsqlCommand(query, con);
            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            List<Activity> activities = new List<Activity>();
            while (rdr.Read())
            {
                activities.Add(new Activity(rdr.GetInt32(0), rdr.GetString(1),
                                    rdr.GetString(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetBoolean(8)));
            }

            return activities;

            /*string query = $"SELECT * FROM activity WHERE location='{city}';";
            SqlConnection con = new SqlConnection(URL);
            SqlCommand command = new SqlCommand(query, con);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();
            command.Dispose();
            con.Close();
            */

        }

        //Retrieves all activities hosted in a specific city within a given time period
        public List<Activity> GetActivities(string city, int monthsForward)
        {
            DateTime currentDate = DateTime.Now;
            DateTime specifiedDate = DateTime.Now.AddMonths(monthsForward);

            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            var query = $"SELECT * FROM activity WHERE location='{city}' AND date BETWEEN {currentDate} AND {specifiedDate};";

            using var cmd = new NpgsqlCommand(query, con);
            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            List<Activity> activities = new List<Activity>();
            while (rdr.Read())
            {
                activities.Add(new Activity(rdr.GetInt32(0), rdr.GetString(1),
                                    rdr.GetString(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetBoolean(8)));
            }

            return activities;

            /*
            string query = $"SELECT * FROM activity WHERE location='{city}' AND date BETWEEN {currentDate} AND {specifiedDate};";
            SqlConnection con = new SqlConnection(URL);
            SqlCommand command = new SqlCommand(query, con);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();
            command.Dispose();
            con.Close();
            */
        }

        //Retrieves specific activities by given ID's
        public List<Activity> GetActivities(List<int> listOfActivityID)
        {
            string query = $"SELECT * FROM activity WHERE id = {listOfActivityID.First()}";

            var sb = new System.Text.StringBuilder();
            foreach (int x in listOfActivityID){
                sb.AppendLine($" OR id = {x}");
            }
            query += sb + ";";

            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            using var cmd = new NpgsqlCommand(query, con);
            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            List<Activity> activities = new List<Activity>();
            while (rdr.Read())
            {
                activities.Add(new Activity(rdr.GetInt32(0), rdr.GetString(1),
                                    rdr.GetString(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetBoolean(8)));
            }

            return activities;

            /*
            SqlConnection con = new SqlConnection(URL);
            SqlCommand command = new SqlCommand(query, con);
            con.Open();
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();
            command.Dispose();
            con.Close();
            */
        }

        //Retrieves activities containing specific tags
        public List<Activity> GetActivitiesByPreference(List<string> listOfPreferences)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            var query = $"SELECT activityid FROM type WHERE tag = {listOfPreferences.First()}";
            var sb = new System.Text.StringBuilder();
            foreach (string x in listOfPreferences.Skip(1))
            {
                sb.AppendLine($" OR tag = {x}");
            }
            query += sb + ";";

            using var cmd = new NpgsqlCommand(query, con);
            using NpgsqlDataReader tagrdr = cmd.ExecuteReader();

            List<int> activityList = new List<int>();
            while (tagrdr.Read())
            {
                activityList.Add(tagrdr.GetInt32(0));
            }

            return GetActivities(activityList);
        }

        //Retrieves all activities created by a specific organization
        public List<Activity> GetUserActivities(int organizerId)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            var query = $"SELECT activityid FROM type WHERE host = {organizerId}";

            using var cmd = new NpgsqlCommand(query, con);
            using NpgsqlDataReader rdr = cmd.ExecuteReader();

            List<Activity> activities = new List<Activity>();
            while (rdr.Read())
            {
                activities.Add(new Activity(rdr.GetInt32(0), rdr.GetString(1),
                rdr.GetString(2), rdr.GetString(3), rdr.GetString(4), rdr.GetString(5), rdr.GetString(6), rdr.GetString(7), rdr.GetBoolean(8)));
            }
            return activities;
        }

        //tarts retrieval of new activities, using API calls
        public void UpdateActivities()
        {
        }

        //Removes activities from the database. Requires proper role
        public void RemoveActivities(List<int> listOfActivityID) 
        {
            var query = $"DELETE FROM activitydb WHERE id = {listOfActivityID.First()}";

            var sb = new System.Text.StringBuilder();
            foreach (int x in listOfActivityID.Skip(1))
            {
                sb.AppendLine($" OR id = {x}");
            }
            query += sb + ";";

            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            using var cmd = new NpgsqlCommand(query, con);
            using NpgsqlDataReader rdr = cmd.ExecuteReader();
        }
    }
}
