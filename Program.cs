using ActivityService.Classes;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
Database db = new Database();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

//Adds an activity to the database
app.MapGet("/AddActivity", (string title, string host, string location, string date, string imageurl, string url, string description) =>
{
    Activity act = new Activity(1, title, host, location, date, imageurl, url, description, true);
    db.AddActivity(act);
    return "Ja det fint";
});

//Retrieves all activities from a given area within a time period.
app.MapGet("/GetActivities/{function}", (string function, string area, List<int> listOfActivityID, int monthsForward) => 
{
    try
    {
        if (function == "area")
        {
            List<Activity> activityList = db.GetActivities(area);
            string json = JsonSerializer.Serialize(activityList);

            return json;
        }
        else if (function == "areaTime")
        {
            List<Activity> activityList = db.GetActivities(area, monthsForward);
            string json = JsonSerializer.Serialize(activityList);

            return json;
        }
        else if (function == "activity")
        {
            List<Activity> activityList = db.GetActivities(listOfActivityID);
            string json = JsonSerializer.Serialize(activityList);

            return json;
        }
        throw new Exception("No valid functionality given!");
    }
    catch (Exception e)
    {
        string err = $"[ACTIVITY DATABASE] Something went wrong when trying to get activities! \n{e.Message}";
        Console.WriteLine(err);
        //return Results.Problem(err, null, 500);
        return err;
    }
});

//Retrieves all activities containing tags based on given interests.
app.MapGet("/GetActivitiesByPreference", (List<string> listOfPreferences) => 
{
    List<Activity> activityList = db.GetActivitiesByPreference(listOfPreferences);
    string json = JsonSerializer.Serialize(activityList);

    return json;
});

//Retrieves all activities organized by the given user.
app.MapGet("/GetUserActivities", (int userID) =>
{
    List<Activity> activityList = db.GetUserActivities(userID);
    string json = JsonSerializer.Serialize(activityList);

    return json;
});

//Starts retrieval of new activities, using API calls.
app.MapGet("/UpdateActivities", () =>
{
    return "notImplemented";
});

//Removes the given activity from the activity database
app.MapGet("/RemoveActivities", (List<int> listOfActivityID) =>
{
    db.RemoveActivities(listOfActivityID);

    return "Activities removed!";
});

app.Run();

