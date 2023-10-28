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
    
    private async Task AddUserMessages(List<ChatMessage> messages, GarthCommandContext context)
    {
        var thread = await ResolveThreadTree(context);
        (thread.Count switch
        {
            > 0 => thread,
            _ => (await context.Channel.GetMessagesAsync(5).FlattenAsync()).Skip(1)
                .Where(x => x.Timestamp >= DateTimeOffset.Now.AddMinutes(-20))
        }).ToList().ForEach(message =>
        {
            messages.Add(
                new ChatMessage(
                    message.Author.Id == _discord.CurrentUser.Id ? "assistant" : "user",
                    FormatDiscordMessageForGpt(message)
                )
            );
        });
        messages.Add(new ChatMessage("user", FormatDiscordMessageForGpt(context.Message)));
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

                // Finally, send the chat request
                var request = new ChatCompletionCreateRequest()
                {
                    Functions = FunctionCallingHelper.GetFunctionDefinitions(gptContext),
                    Messages = messages,
                    Model = Models.Gpt_4
                };

                var reply = await _openAiService.ChatCompletion.CreateCompletion(request);

                if (!reply.Successful)
                {
                    Console.WriteLine("An error occured during a GPT Request");
                    Console.WriteLine(reply.Error.ToString());
                    return;
                }

                var response = reply.Choices.First().Message;
                if (response.FunctionCall is not null)
                {
                    var functionCall = response.FunctionCall;
                    _ = FunctionCallingHelper.CallFunction<Task>(functionCall, gptContext);
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