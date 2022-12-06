using ActivityService.Classes;
using System.Text.Json;
using MySql.Data.MySqlClient;


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
app.MapGet("/GetActivities", (string function, string area, int monthsForward, string jsonActivityList) => 
{
    List<Activity> activityList;
    string json;
    try
    {
        switch (function)
        {
            case "area":
                activityList = db.GetActivities(area);
                json = JsonSerializer.Serialize(activityList);

                return Results.Accepted(json);
            case "areaTime":
                activityList = db.GetActivities(area, monthsForward);
                json = JsonSerializer.Serialize(activityList);

                return Results.Accepted(json);
            case "activity":
                List<int> listOfActivityID = JsonSerializer.Deserialize<List<int>>(jsonActivityList);
                activityList = db.GetActivities(listOfActivityID);
                json = JsonSerializer.Serialize(activityList);

                return Results.Accepted(json);
        }
        throw new Exception("No valid functionality given!");
    }
    catch (Exception e)
    {
        string err = $"[ACTIVITY DATABASE] Something went wrong when trying to get activities! \n{e.Message}";
        return Results.Problem(err);
    }
});

//Retrieves all activities containing tags based on given interests.
app.MapGet("/GetActivitiesByPreference", (string jsonPreferenceList) => 
{
    try
    {
        List<string> listOfPreferences = JsonSerializer.Deserialize<List<string>>(jsonPreferenceList);
        List<Activity> activityList = db.GetActivitiesByPreference(listOfPreferences);
        string json = JsonSerializer.Serialize(activityList);

        return Results.Accepted(json);
    } 
    catch
    {
        return Results.BadRequest("Something went wrong");
    }
    
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
app.MapGet("/RemoveActivities", (string jsonActivityList) =>
{
    try
    {
        List<int> listOfActivityID = JsonSerializer.Deserialize<List<int>>(jsonActivityList);
        db.RemoveActivities(listOfActivityID);
        return Results.Accepted("Activity deleted");
    }
    catch
    {
        return Results.BadRequest("Something went wrong");
    }
    
});

app.Run();

