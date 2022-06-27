using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Garth.DAL;
using Garth.DAL.DomainClasses;
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

        if (content.Split(' ').Length < 2)
            return false;
         
        if (new Random().Next(0, 100) == 1 && content.Split(' ').Length >= 5)
            return true;

        Regex regex = new Regex(@":\S*?garf\S*?:", RegexOptions.IgnoreCase);
        
        return content.ToLower().Contains("garf") && !regex.IsMatch(content);
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
            "dick",
            "penis",
            "vagina",
            "virgin",
            "porn",
            "sex",
            "gay",
            "lesbian",
            "bisexual",
            "smut",
            "ass",
            "virginity"
        };

        if (bannedWords.Any(t => content.ToLower().Contains($" {t} ")))
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
            //"Your name is Garth Santor.\nYou are 58 years old.\nYou teach computer science at Fanshawe college.\n" +
            $"The current date and time is {String.Format("{0:F}", targetTime)} EST" +
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