using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace ActivityService.Classes
{
    public class Activity
    {
        public int id { get; }
        public string title { get; set; }
        public string host { get; set; }
        public string place { get; set; }
        public string date { get; set; }
        public string img { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public bool active { get; set; }
        public string type { get; set; }

        public Activity(int id, string title, string host, string location, string date, string imageurl, string url, string description, bool active, string type)
        {
            this.id = id;
            this.title = title;
            this.host = host;
            this.place = location;
            this.date = date;
            this.img = imageurl;
            this.url = url;
            this.description = description;
            this.active = active;
            this.type = type;
        }

        [JsonConstructor]
        public Activity(string date, string host, string img, string place, string title, string type, string url)
        {
            this.id = 0;
            this.active = true;
            this.description = "";

            this.date = date;
            this.host = host;
            this.img = img;
            this.place = place;
            this.title = title;
            this.type = type;
            this.url = url;
        }
    }
}
