using Newtonsoft.Json;

namespace ChatGPTCommunicator.Models.Function.Parameters;

public class Parameters
{
    [JsonProperty("type")]
    public string Type { get; set; }
    
    [JsonProperty("properties")]
    public Properties.Properties Properties { get; set; }
    
    [JsonProperty("required")]
    public List<string> Required { get; set; }
}