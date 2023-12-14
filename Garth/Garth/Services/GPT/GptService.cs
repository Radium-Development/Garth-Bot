using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;
using Garth.IO;
using Garth.Services.GPT.Functions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.Utilities.FunctionCalling;

namespace Garth.Services.GPT;

public class GptService
{
    private readonly DiscordSocketClient _discord;
    private readonly Configuration.Config _configuration;
    private readonly OpenAIService _openAiService;
    private readonly GarthDbContext _db;
    
    private readonly Regex emoteRegex = new(@":\S*?garf\S*?:", RegexOptions.IgnoreCase);
    
    public GptService(IServiceProvider services)
    {
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _configuration = services.GetRequiredService<Configuration>();
        _openAiService = services.GetRequiredService<OpenAIService>();
        _db = services.GetRequiredService<GarthDbContext>();
        
        _discord.MessageReceived += MessageReceivedAsync;
    }
    
    private async Task<List<IMessage>> ResolveThreadTree(GarthCommandContext ctx) =>
        await ResolveThreadTree(ctx.Guild.Id, ctx.Channel.Id, ctx.Message.Id);

    private string CleanMessage(string msg) =>
        new Regex("\\[[0-9]{19}\\] [a-zA-Z0-9]{0,}: ").Replace(msg, "").Trim();
    
    private async Task<List<IMessage>> ResolveThreadTree(ulong guildId, ulong channelId, ulong messageId)
    {
        var channel = (SocketTextChannel)_discord.GetGuild(guildId).GetChannel(channelId);
        var message = await channel.GetMessageAsync(messageId);
        
        List<IMessage> messages = new() {};

        while (message.Reference != null)
        {
            message = await channel.GetMessageAsync(message.Reference.MessageId.Value);
            messages.Add(message);
        }

        messages.Reverse();
        
        return messages;
    }

    private bool ShouldGptReply(GarthCommandContext context, out bool isExplicitReply, out bool isRandomReply)
    {
        if (context.User.Id == _discord.CurrentUser.Id)
        {
            isExplicitReply = false;
            isRandomReply = false;
            return false;
        }

        var messageContent = context.Message.Content
            .Replace("garf", "Garth", StringComparison.CurrentCultureIgnoreCase);
        
        isExplicitReply = context.Message.Content.Contains("garf", StringComparison.CurrentCultureIgnoreCase) && !emoteRegex.IsMatch(context.Message.Content);
        if (context.Channel is IThreadChannel threadChannel)
        {
            isExplicitReply |= threadChannel.OwnerId == _discord.CurrentUser.Id;
        }
        isExplicitReply |= context?.Message?.ReferencedMessage?.Author?.Id == _discord.CurrentUser.Id;
        isRandomReply = new Random().Next(0, 100) == 1 && messageContent.Split(' ').Length >= 5 && !isExplicitReply;

        return isExplicitReply || isRandomReply;
    }

    private async Task AddPromptMessages(List<ChatMessage> messages)
    {
        var timeUtc = DateTime.UtcNow;
        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
        
        foreach (var ctx in await _db.Contexts!.Where(x => x.Enabled).ToListAsync())
        {
            var ctxValue = ctx.Value
                .Replace("[[date]]", easternTime.ToString("D"))
                .Replace("[[time]]", easternTime.ToString("t"));

            messages.Add(new ChatMessage("system", ctxValue));
        }
    }

    private string FormatDiscordMessageForGpt(IMessage message)
    {
        string content = message.Content.Replace("garf", "Garth", StringComparison.CurrentCultureIgnoreCase);
        return $"[{message.Id}] {message.Author.Username}: {content}";
    }
    
    private Tuple<List<string>, string> ExtractImageUrlsAndCleanString(string text)
    {
        List<string> urls = new List<string>();
        // Regular expression for extracting URLs that end with image file extensions
        string pattern = @"http[s]?://[\S]*\.(jpg|jpeg|png|gif|bmp|webp)";

        MatchCollection matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            urls.Add(match.Value);
        }

        // Remove all matched URLs from the original string
        string cleanedText = Regex.Replace(text, pattern, string.Empty);

        return Tuple.Create(urls, cleanedText);
    }
    
    private async Task AddUserMessages(List<ChatMessage> messages, GarthCommandContext context)
    {
        var thread = await ResolveThreadTree(context);
        (thread.Count switch
        {
            > 0 => thread,
            _ => (await context.Channel.GetMessagesAsync(10).FlattenAsync()).Skip(1)
                .Where(x => x.Timestamp >= DateTimeOffset.Now.AddMinutes(-20))
        }).ToList().ForEach(message =>
        {
            // Check if the bot sent the message
            if (message.Author.Id == _discord.CurrentUser.Id)
            {
                messages.Add(ChatMessage.FromAssistant(FormatDiscordMessageForGpt(message)));
            }
            else
            {
                var (urls, messageText) = ExtractImageUrlsAndCleanString(FormatDiscordMessageForGpt(message));
                var messageContents = new List<MessageContent>()
                {
                    MessageContent.TextContent(messageText)
                };
        
                // Add all images via URL
                foreach(var url in urls)
                    messageContents.Add(MessageContent.ImageUrlContent(url, "high"));
        
                // Add all images via attachment
                foreach(var attachment in message.Attachments.Where(x => x.ContentType.StartsWith("image")))
                    messageContents.Add(MessageContent.ImageUrlContent(attachment.Url, "high"));
        
                messages.Add(ChatMessage.FromUser(messageContents));
            }
            
        });
        
        var (urls, messageText) = ExtractImageUrlsAndCleanString(FormatDiscordMessageForGpt(context.Message));
        var messageContents = new List<MessageContent>()
        {
            MessageContent.TextContent(messageText)
        };
        
        // Add all images via URL
        foreach(var url in urls)
            messageContents.Add(MessageContent.ImageUrlContent(url, "high"));
        
        // Add all images via attachment
        foreach(var attachment in context.Message.Attachments.Where(x => x.ContentType.StartsWith("image")))
            messageContents.Add(MessageContent.ImageUrlContent(attachment.Url, "high"));
        
        messages.Add(ChatMessage.FromUser(messageContents));
    }
    
    private Task MessageReceivedAsync(SocketMessage arg)
    {
        _ = Task.Run(async () =>
        {
            GarthCommandContext context = new(_discord, (SocketUserMessage)arg, _db);
            GptContext gptContext = new(context);

            if (!ShouldGptReply(context, out bool isExplicitReply, out bool isRandomReply))
                return;

            using var state = context.Channel.EnterTypingState();
            try
            {
                List<ChatMessage> messages = new();

                await AddPromptMessages(messages);

                if (isRandomReply)
                    messages.Add(new ChatMessage("system",
                        "You are currently chiming in randomly without being mentioned. Give a more concise answer and don't get too technical."));

                await AddUserMessages(messages, context);

                bool hasImages = messages.Any(x => x.Contents != null && x.Contents.Any(c => c.Type == "image_url"));
                
                // Finally, send the chat request
                var request = new ChatCompletionCreateRequest
                {
                    Tools = hasImages ? null : FunctionCallingHelper.GetFunctionDefinitions(gptContext).Select(function => new ToolDefinition
                    {
                        FunctionsAsObject = function,
                        Type = StaticValues.CompletionStatics.ToolType.Function
                    }).ToList(),
                    Messages = messages,
                    Model = hasImages ? Models.Gpt_4_vision_preview : Models.Gpt_4_1106_preview,
                    MaxTokens = 4096
                };

                var reply = await _openAiService.ChatCompletion.CreateCompletion(request);
                if (!reply.Successful)
                {
                    Console.WriteLine("An error occured during a GPT Request");
                    Console.WriteLine(reply.Error.Message);
                    return;
                }

                var response = reply.Choices.First().Message;
                if (response.ToolCalls != null && response.ToolCalls.Any())
                {
                    foreach(var toolCall in response.ToolCalls)
                    {
                        var functionCall = toolCall.FunctionCall;
                        _ = FunctionCallingHelper.CallFunction<Task>(functionCall, gptContext);
                    }
                }
                else
                {
                    await context.Message.ReplyAsync(CleanMessage(response.Content));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                state.Dispose();
            }
        });
        return Task.CompletedTask;
    }
}