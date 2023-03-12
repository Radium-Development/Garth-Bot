using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ChatGPTCommunicator.Models;

public class Message
{
    [JsonProperty("role")]
    [JsonConverter(typeof(StringEnumConverter))]
    public MessageRole Role { get; set; }
        
    [JsonProperty("content")]
    public string Content { get; set; }

    public Message(MessageRole role, string content)
    {
        Role = role;
        Content = content;
    }
}