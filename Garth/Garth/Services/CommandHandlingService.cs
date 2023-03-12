using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ChatGPTCommunicator;
using ChatGPTCommunicator.Models;
using ChatGPTCommunicator.Requests.Completion;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;
using Garth.IO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NTextCat;
using Renci.SshNet.Messages;
using Message = Renci.SshNet.Messages.Message;

namespace Garth.Services;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;
    private readonly Configuration.Config _configuration;
    private readonly GptService _gptService;
    private readonly GarthDbContext _db;
    private readonly CommandHistoryDAO _commandHistoryDAO;
    private readonly ChatGPT _chatGpt;
    private readonly Regex emoteRegex = new Regex(@":\S*?garf\S*?:", RegexOptions.IgnoreCase);
    
    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _configuration = services.GetRequiredService<Configuration>();
        _gptService = services.GetRequiredService<GptService>();
        _db = services.GetRequiredService<GarthDbContext>();
        _commandHistoryDAO = new CommandHistoryDAO(_db);
        _chatGpt = services.GetRequiredService<ChatGPT>();
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
        Console.WriteLine("DoChatGptWork()");
        
        var messageContent = context.Message.Content
            .Replace("garf", "Garth", StringComparison.CurrentCultureIgnoreCase);

        if (messageContent.Split(' ').Length < 2)
            return;
        
        bool askGpt = new Random().Next(0, 100) == 1 && messageContent.Split(' ').Length >= 5; // Random Chance of Reply
        bool isRandomReply = askGpt;
        askGpt |= context.Message.Content.Contains("garf", StringComparison.CurrentCultureIgnoreCase) && !emoteRegex.IsMatch(messageContent);
        askGpt |= context.Message.ReferencedMessage.Author.Id == _discord.CurrentUser.Id;

        // user didn't mention garth, and it was not a random chance
        if (!askGpt)
            return;

        using (context.Channel.EnterTypingState())
        {
            CompletionRequestBuilder builder = new CompletionRequestBuilder()
                .AddMessage(MessageRole.system,
                    "Garth is chill college student who studies computer science with his friends at Fanshawe College. He sometimes gives sarcastic responses and is typically rather funny.");

            // Foreach message in a thread, add them to the context
            var thread = await ResolveThreadTree(context);
            (thread.Count switch
            {
                > 0 => thread,
                _ => (await context.Channel.GetMessagesAsync(5).FlattenAsync()).Skip(1)
                    .Where(x => x.Timestamp >= DateTimeOffset.Now.AddMinutes(-20))
            }).ToList().ForEach(message => builder.AddMessage(MessageRole.user, $"{message.Author.Username}: {message.Content}"));

            builder.AddMessage(MessageRole.user, $"{context.Message.Author.Username}: {context.Message.Content}");

            var chatGptResponse = await _chatGpt.SendAsync(builder.Build());

            // Print out the current ChatGPT request
            //_ = context.Channel.SendMessageAsync(
            //    $"```{string.Join("\n", builder.Build().Messages.Select(x => x.Content))}```");

            var responseMessage = chatGptResponse!.Choices.First().Message.Content
                .Replace("Garth: ", "");

            _ = context.Channel.SendMessageAsync(responseMessage);
        }
    }

    private Task _discord_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2, ISocketMessageChannel arg3)
    {
        return MessageReceivedAsync(arg2);
    }

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

        Console.WriteLine("MessageRecievedAsync()");
        
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

        Console.WriteLine("1111()");
        
        if (shouldReturn)
            if (message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                shouldReturn = false;

        var context = new GarthCommandContext(_discord, message, _db);

        Console.WriteLine("22222()");
        if (shouldReturn)
        {
            Console.WriteLine("33333()");
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
        
        Console.WriteLine("44444()");
        if (!cmdResult.IsSuccess)
        {
            Console.WriteLine("55555()");
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