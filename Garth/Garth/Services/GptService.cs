using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ChatGPTCommunicator.Models;
using ChatGPTCommunicator.Requests.Completion;
using Garth.DAL;
using Garth.DAL.DomainClasses;
using Garth.Enums;
using Garth.IO;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using Shared.Helpers;
using CompletionRequest = OpenAI_API.CompletionRequest;

namespace Garth.Services;

public class GptService
{
    private readonly OpenAIAPI? _api;
    private readonly GarthDbContext _db;
    private readonly Configuration.Config _config;
    private readonly List<Engine> _engines;
    
    public GptService(GarthDbContext context, Configuration.Config config)
    {
        _db = context;
        _config = config;
        
        var openAiToken = EnvironmentVariables.Get("OPENAI_KEY");
        
        if (openAiToken is null)
            return;
        
        _api = new OpenAIAPI(openAiToken);

        _engines = _api.Engines.GetEnginesAsync().GetAwaiter().GetResult();
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

    public async Task<GptResponse> GetResponse(string content, string sender, Model model = Model.Text)
    {
        var engine = (model) switch
        {
            Model.Codex => "code-davinci-002",
            Model.Text => "gpt-3.5-turbo-0301"//"text-davinci-002"
        };
        
        _api!.UsingEngine = _engines.FirstOrDefault(t => t.EngineName == engine, Engine.Davinci);
        
        if (_api is null)
            return new GptResponse
            {
                Success = false,
                Error = "GPT-3 Service Failed to start"
            };

        List<string> bannedWords = (await _db.Blacklist!.ToListAsync()).Select(x => x.Value).ToList();

        var blacklistedWordsWithinContent = bannedWords.Where(t => content.ToLower().Contains($" {t} "));
        if (blacklistedWordsWithinContent.Any())
            return new GptResponse()
            {
                Success = false,
                Error = "OpenAI's GPT-3 usage policy kindly requests that topics involving race, beliefs, or religion and any other foul content be avoided.\n\nThis message was not produced by GPT-3, but instead a blacklist that prevents specific keywords from being sent to GPT.3. Trying not to bypass this filter with weird tricks would be appreciated.",
                BlacklistWords = blacklistedWordsWithinContent
            };

        return (model) switch
        {
            Model.Codex => await CodexResponse(content),
            Model.Text => await TextResponse(content, sender)
        };
    }

    private async Task<GptResponse> CodexResponse(string content)
    {
        var completionResult = await _api!.Completions.CreateCompletionAsync(
            content,
            max_tokens: 300, 
            temperature: 0.0,
            top_p: 1,
            frequencyPenalty: 0,
            presencePenalty: 0
        );

        var response = completionResult.Completions.First().Text;
            
        return new GptResponse
        {
            Success = true,
            Response = response
        };
    }

    private async Task<GptResponse> TextResponse(string content, string sender)
    {
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
        var targetTime = TimeZoneInfo.ConvertTime(DateTime.Now, easternStandardTime!);
        
        string finalMessage =
            /*"Your name is Garth Santor.\nYou are 58 years old.\nYou teach computer science at Fanshawe college.\n" +
            $"The current date and time is {String.Format("{0:F}", targetTime)} EST" +
            string.Join("\n", contexts.Select(t => t.Value)) + "\n\n---\n\n" +*/
            (new Random().Next(0, 3) == 1 ? "Garth is a chatbot that reluctantly answers questions with sarcastic responses.\n" : "Garth is a chatbot that helps answer questions.\n") +
            content
                .Replace("Garf", "Garth")
                .Replace("garf", "garth")
                .Trim(',')
                .Trim();
        
        Console.WriteLine(finalMessage + "\n\n");
        
        var completionResult = await _api!.Completions.CreateCompletionAsync(
            finalMessage,
            max_tokens: 600, 
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
    public IEnumerable<string> BlacklistWords { get; set; }
}