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
        public List<string> type {get; set; }
        public bool active { get; set; }

        public Activity(int id, string title, string host, string place, string date, string img, string url, string description, List<string> type, bool active)
        {
            this.id = id;
            this.title = title;
            this.host = host;
            this.place = place;
            this.date = date;
            this.img = img;
            this.url = url;
            this.description = description;
            this.type = type;
            this.active = active;
        }
    }
}
