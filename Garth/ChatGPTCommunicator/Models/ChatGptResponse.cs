using Newtonsoft.Json;

namespace ChatGPTCommunicator.Models;

public class ChatGptResponse
{
    [JsonProperty("id")]
    public string Id { get; private set; }
    
    [JsonProperty("object")]
    public string Object { get; private set; }

    [JsonProperty("created")]
    public long _created { get; private set; }

    [JsonIgnore] 
    public DateTime Created =>
        new(ticks: _created);
    
    [JsonProperty("model")]
    public string Model { get; private set; }

    [JsonProperty("usage")]
    public TokenUsage Usage { get; private set; }
    
    [JsonProperty("choices")]
    public List<Choice> Choices { get; private set; }
    
    public class TokenUsage
    {
        [JsonProperty("prompt_tokens")]
        public uint PromptTokens { get; private set; }

        [JsonProperty("completion_tokens")]
        public uint CompletionTokens { get; private set; }
        
        [JsonProperty("total_tokens")]
        public uint TotalTokens { get; private set; }
    }

    public class Choice
    {
        [JsonProperty("message")]
        public Message Message { get; private set; }
        
        [JsonProperty("finish_reason")]
        public string FinishReason { get; private set; }
        
        [JsonProperty("index")]
        public int Index { get; private set; }
    }
}