using Newtonsoft.Json;

namespace ChatGPTCommunicator.Models.Function.Parameters.Properties;

public class Properties
{
    [JsonProperty("location")]
    public Location.Location Location { get; set; }
    
    [JsonProperty("unit")]
    public Unit.Unit Unit { get; set; }
}