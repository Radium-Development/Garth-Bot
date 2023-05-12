using System.Net;
using Newtonsoft.Json;
using Shared.Helpers;

namespace Garth.Helpers;

public class Calendar
{
    private static readonly string URL_BASE = "https://folconnect.dombi.ca/api/v1/";
    
    public static async Task<List<CalEvent>> Fetch(string url)
    {
        using WebClient webClient = new();
        var json = await webClient.DownloadStringTaskAsync(URL_BASE + url + $"?username={EnvironmentVariables.Get("FOL_USERNAME")}&password={EnvironmentVariables.Get("FOL_PASSWORD")}");
        return JsonConvert.DeserializeObject<List<CalEvent>>(json);
    }

    public static Task<List<CalEvent>> GetUpcomingEvents() =>
        Fetch("upcoming");
    
    public static Task<List<CalEvent>> GetEventsToday() =>
        Fetch("today");
    
    public static Task<List<CalEvent>> GetEventsTomorrow() =>
        Fetch("tomorrow");
}