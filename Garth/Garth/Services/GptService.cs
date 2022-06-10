using System.Runtime.InteropServices;
using Garth.DAL;
using Garth.DAL.DAO.DomainClasses;
using Garth.IO;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;

namespace Garth.Services;

public class GptService
{
    private readonly OpenAIAPI? _api;
    private readonly GarthDbContext _db;
    private readonly Configuration.Config _config;
    
    public GptService(GarthDbContext context, Configuration.Config config)
    {
        _db = context;
        _config = config;
        
        var openAiToken = Environment.GetEnvironmentVariable("OPENAI_KEY", EnvironmentVariableTarget.Process); 
        openAiToken ??= Environment.GetEnvironmentVariable("OPENAI_KEY", EnvironmentVariableTarget.User); 
        openAiToken ??= Environment.GetEnvironmentVariable("OPENAI_KEY", EnvironmentVariableTarget.Machine);
        
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
         
        //if (new Random().Next(0, 75) == 1 && content.Split(' ').Length >= 5)
        //    return true;
        
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

    public async Task<GptResponse> GetResponse(string content, string sender)
    {
        if (_api is null)
            return new GptResponse
            {
                Success = false,
                Error = "GPT-3 Service Failed to start"
            };

        string[] bannedWords = new string[]
        {
            "jesus",
            "god",
            "religion",
            "fuck",
            "shit",
            "bitch",
            "cunt",
            "jew",
            "cock",
            "penis",
            "virgin",
            "porn",
            "sex",
            "gay",
            "lesbian",
            "bisexual"
        };

        if (bannedWords.Any(t => content.ToLower().Contains(t)))
            return new GptResponse()
            {
                Success = false,
                Error =
                    "OpenAI's GPT-3 usage policy kindly requests that topics involving race, beliefs, or religion and any other foul content be avoided.\n\nThis message was not produced by GPT-3, but instead a blacklist that prevents specific keywords from being sent to GPT.3. Trying not to bypass this filter with weird tricks would be appreciated."
            };

        List<Context> contexts = await _db.Contexts!.ToListAsync();
        
        TimeZoneInfo easternStandardTime = null;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            easternStandardTime = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            easternStandardTime = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            easternStandardTime = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        }
        var targetTime = TimeZoneInfo.ConvertTime(DateTime.Now, easternStandardTime);
        
        string finalMessage =
            "Your name is Garth Santor.\nYou are 58 years old.\nYou teach computer science at Fanshawe college.\n" +
            $"The current date and time is {String.Format("{0:F}", targetTime)}" +
            string.Join("\n", contexts.Select(t => t.Value)) + "\n\n---\n\n" +
            content
                .Replace("Garf", "Garth")
                .Replace("garf", "garth")
                .Trim(',')
                .Trim();
        
        Console.WriteLine(finalMessage + "\n\n");
        
        var completionResult = await _api.Completions.CreateCompletionAsync(
            finalMessage,
            max_tokens: 300, 
            temperature: 0.9,
            top_p: 1,
            frequencyPenalty: _config.FrequencyPenalty,
            presencePenalty: _config.PresencePenalty
        );

        var response = completionResult.Completions.First().Text.Split("AI: ")[0].Split("Garth: ")[0].Split(sender + ": ")[0];
            
        return new GptResponse
        {
            Success = true,
            Response = response
        };
    }
}

public class GptResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Response { get; set; }
}