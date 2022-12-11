using ActivityService.Classes;
using System.Text.Json;
using MySql.Data.MySqlClient;


var builder = WebApplication.CreateBuilder(args);
Database db = new Database();

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

//Adds an activity to the database
app.MapPost("/AddActivity", (string title, string host, string location, string date, string imageurl, string url, string description, string type) =>
{
    Activity act = new Activity(1, title, host, location, date, imageurl, url, description, true, type);
    try
    {
        db.AddActivity(act);
        return Results.Ok();
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
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
                return Results.Json(activityList);
            case "areaTime":
                activityList = db.GetActivities(area, monthsForward);
                return Results.Json(activityList);
            case "activity":
                if (jsonActivityList != null)
                {
                    var listOfActivityID = JsonSerializer.Deserialize<List<int>>(jsonActivityList);
                    activityList = db.GetActivities(listOfActivityID!);
                    return Results.Json(activityList);
                }
                else
                    throw new ArgumentNullException();
        }
        return Results.BadRequest("no valid function given");
    }
    catch (ArgumentNullException e)
    {
        string err = $"[ACTIVITY DATABASE][/GetActivities] ArgumentNullException. Activity list must not be null! \n{e.Message}";
        return Results.Problem(err);
    }
    catch (JsonException e)
    {
        string err = $"[ACTIVITY DATABASE][/GetActivities] JsonException. Invalid Json string! \n{e.Message}";
        return Results.Problem(err);
    }
    catch (NotSupportedException e)
    {
        string err = $"[ACTIVITY DATABASE][/GetActivities] NotSupportedException. No implementation exists for the invoked method or property! \n{e.Message}";
        return Results.Problem(err);
    }
    catch (Exception e)
    {
        string err = $"[ACTIVITY DATABASE][/GetActivities] Something went wrong when trying to get activities! \n{e.Message}";
        return Results.Problem(err);
    }
});

//Retrieves all activities containing tags based on given interests.
app.MapGet("/GetActivitiesByPreference", (string jsonPreferenceList) => 
{
    try
    {
        var listOfPreferences = JsonSerializer.Deserialize<Dictionary<string,int>>(jsonPreferenceList);
        //TODO TEMP FIX
        List<string> listOfKeys = listOfPreferences.Keys.ToList();
        List<Activity> activityList = db.GetActivitiesByPreference(listOfKeys!);
        
        return Results.Json(activityList);
    } 
    catch (Exception e)
    {
        string err = $"[ACTIVITY DATABASE][/GetActivitiesByPreference] Something went wrong when trying to get activities! \n{e.Message}";
        return Results.Problem(err);
    }
    
});

//Retrieves all activities organized by the given user.
app.MapGet("/GetUserActivities", (int userID) =>
{
    try
    {
        List<Activity> activityList = db.GetUserActivities(userID);
        return Results.Json(activityList);

    }
    catch
    {
        string err = $"[ACTIVITY DATABASE][/GetActivitiesByPreference] Something went wrong when trying to get activities!";
        return Results.Problem(err);
    }
});

//Starts retrieval of new activities, using API calls.
app.MapPost("/UpdateActivities", () =>
{
    try
    {
        db.UpdateActivities();
        return Results.Ok();
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
});

//Removes the given activity from the activity database
app.MapDelete("/RemoveActivities", (string jsonActivityList) =>
{
    try
    {
        var listOfActivityID = JsonSerializer.Deserialize<List<int>>(jsonActivityList);
        db.RemoveActivities(listOfActivityID!);
        return Results.Ok("Activities deleted");
    }
    catch
    {
        return Results.BadRequest($"[ACTIVITY DATABASE][/RemoveActivities] Something went wrong when trying to remove activities!");
    }
    
});

app.Run();

