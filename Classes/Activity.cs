using Microsoft.AspNetCore.Identity;

namespace ActivityService.Classes
{
    public class Activity
    {
        public int id { get; }
        public string title { get; set; }
        public string host { get; set; }
        public string location { get; set; }
        public string date { get; set; }
        public string imageurl { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public bool active { get; set; }

        public Activity(int id, string title, string host, string location, string date, string imageurl, string url, string description, bool active)
        {
            this.id = id;
            this.title = title;
            this.host = host;
            this.location = location;
            this.date = date;
            this.imageurl = imageurl;
            this.url = url;
            this.description = description;
            this.active = active;
        }
    }
}
