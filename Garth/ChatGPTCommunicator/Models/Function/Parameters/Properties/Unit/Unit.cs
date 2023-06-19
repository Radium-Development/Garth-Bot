using Newtonsoft.Json;

namespace ChatGPTCommunicator.Models.Function.Parameters.Properties.Unit;

public class Unit
{
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("enum")]
    public List<string> @Enum { get; set; }
}