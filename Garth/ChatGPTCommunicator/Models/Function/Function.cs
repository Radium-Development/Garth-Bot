using Newtonsoft.Json;

namespace ChatGPTCommunicator.Models.Function;

public class Function
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("parameters")]
    public Parameters.Parameters Parameters { get; set; }
}