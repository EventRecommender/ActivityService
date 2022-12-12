using Microsoft.AspNetCore.Identity;

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
        public ActivityType type { get; set; }
        public bool active { get; set; }

        public Activity(string title, string host, string place, string date, string img, string url, string description = "", string type = "", bool active = true, int id = -1)
        {
            this.id = id;
            this.title = title;
            this.host = host;
            this.place = place;
            this.date = date;
            this.img = img;
            this.url = url;
            this.type.types.Add(type);
            this.description = description;
            this.active = active;
        }
    }
}
