using Newtonsoft.Json;

namespace ChatGPTCommunicator.Models;

public class ModerationResponse
{
    [JsonProperty("id")]
    public string Id { get; }
    
    [JsonProperty("model")]
    public string Model { get; }

    [JsonProperty("results")]
    public List<Result> Results { get; }
    
    public class Result
    {
        [JsonProperty("categories")]
        public ResultCategory Categories { get; }
        
        [JsonProperty("category_scores")]
        public ResultCategoryScores CategoryScores { get; }
        
        [JsonProperty("flagged")]
        public bool Flagged { get; }
        
        public class ResultCategory
        {
            [JsonProperty("hate")]
            public bool Hate { get; }
            
            [JsonProperty("hate/threatening")]
            public bool HateAndThreatening { get; }
            
            [JsonProperty("self-harm")]
            public bool SelfHarm { get; }
            
            [JsonProperty("sexual")]
            public bool Sexual { get; }
            
            [JsonProperty("sexual/minors")]
            public bool SexualAndMinors { get; }
            
            [JsonProperty("violence")]
            public bool Violence { get; }
            
            [JsonProperty("violence/graphic")]
            public bool ViolenceAndGraphic { get; }
        }

        public class ResultCategoryScores
        {
            [JsonProperty("hate")]
            public double Hate { get; }
            
            [JsonProperty("hate/threatening")]
            public double HateAndThreatening { get; }
            
            [JsonProperty("self-harm")]
            public double SelfHarm { get; }
            
            [JsonProperty("sexual")]
            public double Sexual { get; }
            
            [JsonProperty("sexual/minors")]
            public double SexualAndMinors { get; }
            
            [JsonProperty("violence")]
            public double Violence { get; }
            
            [JsonProperty("violence/graphic")]
            public double ViolenceAndGraphic { get; }
        }
    }
}