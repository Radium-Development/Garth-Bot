using OpenAI_API;

namespace Garth.Services;

public class GptService
{
    private readonly OpenAIAPI? _api;
    
    public GptService()
    {
        var openAiToken = Environment.GetEnvironmentVariable("OPENAI_KEY", EnvironmentVariableTarget.User);

        if (openAiToken is null)
            return;
        
        _api = new OpenAIAPI(openAiToken);

        var engines = _api.Engines.GetEnginesAsync().GetAwaiter().GetResult();
        
        _api.UsingEngine = engines.FirstOrDefault(t => t.EngineName == "text-davinci-002", Engine.Davinci);
    }

    public async Task<bool> IsAskingGarth(string content)
    {
        if (_api is null)
            return false;

        if (content.Split(' ').Length < 4)
            return false;
        
        if (new Random().Next(0, 75) == 1 && content.Split(' ').Length >= 5)
            return true;
        
        return content.ToLower().Contains("garf");
        
        // I know the remaining code doesn't run, but I'm not sure it's useless yet. Going to just keep it for now
        // - Erik
        
        if (!content.ToLower().Contains("garf"))
            return false;

        string question = $"Q: Is the following phrase asking Garth a question? \"{content.Trim()}\"\nA:";
        
        var completionResult = await _api.Completions.CreateCompletionAsync(
            question,
            max_tokens: 100,
            temperature: 0,
            top_p: 1,
            frequencyPenalty: 0,
            presencePenalty: 0,
            stopSequences: new [] {"\n"}
            );
        return completionResult.Completions.Any(t => t.Text.Contains("Yes"));
    }

    public async Task<string> GetResponse(string content)
    {
        if (_api is null)
            return "";
        
        var completionResult = await _api.Completions.CreateCompletionAsync(
            content
                .Replace("Garf", "")
                .Replace("garf", "")
                .Trim(',')
                .Trim(),
            max_tokens: 500,
            temperature: 0.9,
            top_p: 1,
            frequencyPenalty: 0,
            presencePenalty: 0.6
        );

        var response = completionResult.Completions.First().Text;
            
        return response;
    }
}