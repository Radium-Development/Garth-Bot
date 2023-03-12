using ChatGPTCommunicator.Models;
using Newtonsoft.Json;

namespace ChatGPTCommunicator.Requests.Completion;

public class CompletionRequest
{
    [JsonProperty("model")]
    public string Model { get; internal set; } = "gpt-3.5-turbo";

    [JsonProperty("messages")]
    public List<Message> Messages { get; internal set; } = new();
}