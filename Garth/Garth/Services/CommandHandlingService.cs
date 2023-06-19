using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;
using Garth.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTextCat;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.ChatFunctions;
using OpenAI_API.Models;
using Renci.SshNet.Messages;
using DbLoggerCategory = Arch.EntityFrameworkCore.DbLoggerCategory;
using Message = Renci.SshNet.Messages.Message;

namespace Garth.Services;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;
    private readonly Configuration.Config _configuration;
    private readonly OpenAIAPI _openAiapi;
    private readonly GarthDbContext _db;
    private readonly CommandHistoryDAO _commandHistoryDAO;
    private readonly Regex emoteRegex = new Regex(@":\S*?garf\S*?:", RegexOptions.IgnoreCase);
    
    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _configuration = services.GetRequiredService<Configuration>();
        _openAiapi = services.GetRequiredService<OpenAIAPI>();
        _db = services.GetRequiredService<GarthDbContext>();
        _commandHistoryDAO = new CommandHistoryDAO(_db);
        _services = services;

        _commands.Log += (s) =>
        {
            Console.WriteLine(s.ToString());
            return Task.CompletedTask;
        };
        
        // Hook CommandExecuted to handle post-command-execution logic.
        _commands.CommandExecuted += CommandExecutedAsync;
        // Hook MessageReceived so we can process each message to see
        // if it qualifies as a command.
        _discord.MessageReceived += MessageReceivedAsync;
        _discord.MessageUpdated += _discord_MessageUpdated;
    }

    private async Task<List<IMessage>> ResolveThreadTree(GarthCommandContext ctx) =>
        await ResolveThreadTree(ctx.Guild.Id, ctx.Channel.Id, ctx.Message.Id);
    
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
    
    private async Task DoChatGptWork(GarthCommandContext context)
    {
        var messageContent = context.Message.Content
            .Replace("garf", "Garth", StringComparison.CurrentCultureIgnoreCase);
        
        bool askGpt = new Random().Next(0, 100) == 1 && messageContent.Split(' ').Length >= 5; // Random Chance of Reply
        bool isRandomReply = askGpt;
        askGpt |= context.Message.Content.Contains("garf", StringComparison.CurrentCultureIgnoreCase) && !emoteRegex.IsMatch(context.Message.Content);
        askGpt |= context?.Message?.ReferencedMessage?.Author?.Id == _discord.CurrentUser.Id;

        // user didn't mention garth, and it was not a random chance
        if (!askGpt)
            return;

        var chat = _openAiapi.Chat.CreateConversation(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo0613,
            /*Function_Call = new Function_Call
            {
                Name = "auto"
            }*/
        });
        
        var timeUtc = DateTime.UtcNow;
        TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
        
        foreach (var ctx in await _db.Contexts!.Where(x => x.Enabled).ToListAsync())
        {
            var ctxValue = ctx.Value
                .Replace("[[date]]", easternTime.ToString("D"))
                .Replace("[[time]]", easternTime.ToString("t"));

            chat.AppendSystemMessage(ctxValue);
        }
        
        if (isRandomReply)
            chat.AppendSystemMessage("You are currently just chiming in randomly. Give a more concise answer and don't get too technical.");
            
        // Foreach message in a thread, add them to the context
        var thread = await ResolveThreadTree(context);
        (thread.Count switch
        {
            > 0 => thread,
            _ => (await context.Channel.GetMessagesAsync(5).FlattenAsync()).Skip(1)
                .Where(x => x.Timestamp >= DateTimeOffset.Now.AddMinutes(-20))
        }).ToList().ForEach(message => chat.AppendMessage(message.Author.Id == _discord.CurrentUser.Id ? ChatMessageRole.Assistant : ChatMessageRole.User, $"{message.Author.Username}: {message.Content.Replace("garf", "Garth", StringComparison.CurrentCultureIgnoreCase)}"));

        chat.AppendUserInput($"{context!.Message?.Author.Username}: {messageContent}");
        var functions = new List<Function>();
        functions.Add(new Function
        {
            Name = "generate_image",
            Description = "Generate an image using Dall-e 2",
            Parameters = JObject.Parse(@"{
                ""type"": ""object"",
                ""properties"": {
                    ""prompt"": {
                        ""type"": ""string"",
                        ""description"": ""The text prompt to use which describes the contents of the image""
                    }
                },
                ""required"": [ ""prompt"" ]
            }")
        });
        //chat.RequestParameters.Functions = functions;
        Emote loadingEmote = Emote.Parse("<a:loadwheel:1120405941641281649>");
        
        var responseMessage =
            await context.Channel.SendMessageAsync("" + loadingEmote, messageReference: GarthModuleBase.CreateMessageReference(context));

        StringBuilder message = new StringBuilder();
        DateTime lastEdit = DateTime.Now;
        await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
        {
            message.Append(res);
            
            if (message.Length >= 10 && lastEdit.AddSeconds(1) <= DateTime.Now && message.Length < 2000)
            {
                lastEdit = DateTime.Now;
                await responseMessage.ModifyAsync(x =>
                    x.Content = message.ToString().Replace("Garth: ", "").Replace("Garth 2.0: ", "") + "... " + loadingEmote);
            }
        }

        string content = message.ToString().Replace("Garth 2.0: ", "").Replace("Garth: ", "");
        if (content.Length > 2000)
        {
            content = content.Substring(0, 1999);
        }
        _ = responseMessage.ModifyAsync(x => x.Content = content);
    }

    private Task _discord_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3) =>
        MessageReceivedAsync(arg2);

    public async Task InitializeAsync()
    {
        // Register modules that are public and inherit ModuleBase<T>.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (!(rawMessage is SocketUserMessage message)) return;
        if (message.Source != MessageSource.User) return;
        
        // This value holds the offset where the prefix ends
        var argPos = 0;
        // Perform prefix check.
        bool shouldReturn = true;
        foreach (var prefix in _configuration.Prefixes!)
        {
            if (message.HasStringPrefix(prefix, ref argPos))
            {
                shouldReturn = false;
                break;
            }
        }
        
        if (shouldReturn)
            if (message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                shouldReturn = false;

        var context = new GarthCommandContext(_discord, message, _db);

        if (shouldReturn)
        {
            _ = DoChatGptWork(context);
            _ = ReplyToInlineTags(context);
            return;
        }

        // Perform the execution of the command. In this method,
        // the command service will perform precondition and parsing check
        // then execute the command if one is matched.
        var cmdResult = await _commands.ExecuteAsync(context, argPos, _services);
        // Note that normally a result will be returned by this format, but here
        // we will handle the result in CommandExecutedAsync,
        
        if (!cmdResult.IsSuccess)
        {
            _ = DoChatGptWork(context);
        }
    }

    public async Task ReplyToInlineTags(GarthCommandContext context)
    {
        var regexMatches = Regex.Matches(context.Message.Content, "\\$+([A-Za-z0-9!.#@$%^&()]+)");

        if (regexMatches.Count == 0)
            return;

        TagDAO tagDao = new(_db);
        
        foreach (Match match in regexMatches)
        {
            if (context.Channel is IGuildChannel channel)
            {
                var tag = await tagDao.GetByName(match.Groups[1].Value);
                
                if(tag == null)
                    continue;

                var reference = GarthModuleBase.CreateMessageReference(context.Guild.Id, context.Message);
                
                if (tag.IsFile)
                {
                    await using MemoryStream stream = new MemoryStream(Convert.FromBase64String(tag.Content!));
                    await context.Channel.SendFileAsync(stream, tag.FileName, messageReference: reference);
                }
                else
                    await context.Channel.SendMessageAsync(tag.Content!, messageReference: reference);
            }
        }
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        // command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;

        var Command = command.Value;
        
        _commandHistoryDAO.Add(new CommandHistory
        {
            Module = Command.Module.Name,
            Name = Command.Name,
            Status = result.IsSuccess ? "Success" : "Failure",
            ErrorReason = result.ErrorReason,
            Error = result.Error.ToString(),
            UserId = context.User.Id,
            User = $"{context.User.Username}#{context.User.Discriminator}",
            Guild = context.Guild?.Name ?? null,
            GuildId = context.Guild?.Id ?? null,
            Channel = context.Channel.Name,
            ChannelId = context.Channel.Id,
            FullCommand = context.Message.Content,
            MessageId = context.Message.Id,
            Timestamp = context.Message.Timestamp,
            Environment = Environment.MachineName
        });

        // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
        {
            return;
        }

        // the command failed, let's notify the user that something happened.
        await context.Channel.SendMessageAsync(embed: EmbedHelper.Error(result.ToString()));
    }
}