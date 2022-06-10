using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO.DAO;
using Garth.IO;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet.Messages;

namespace Garth.Services;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;
    private readonly Configuration.Config _configuration;
    private readonly GptService _gptService;
    private readonly GarthDbContext _db;

    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _configuration = services.GetRequiredService<Configuration>();
        _gptService = services.GetRequiredService<GptService>();
        _db = services.GetRequiredService<GarthDbContext>();
        _services = services;

        // Hook CommandExecuted to handle post-command-execution logic.
        _commands.CommandExecuted += CommandExecutedAsync;
        // Hook MessageReceived so we can process each message to see
        // if it qualifies as a command.
        _discord.MessageReceived += MessageReceivedAsync;
        _discord.MessageUpdated += _discord_MessageUpdated;
    }

    private Task _discord_MessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2,
        ISocketMessageChannel arg3)
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

        // This value holds the offset where the prefix ends
        var argPos = 0;
        // Perform prefix check.
        bool shouldReturn = true;
        foreach (var prefix in _configuration.Prefixes!)
        {
            if (message.HasStringPrefix(prefix + " ", ref argPos))
            {
                shouldReturn = false;
                break;
            }
        }

        if (shouldReturn)
            if (message.HasMentionPrefix(_discord.CurrentUser, ref argPos))
                shouldReturn = false;

        var context = new SocketCommandContext(_discord, message);

        if (shouldReturn)
        {
            _ = DoGptWork(context);
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
            _ = DoGptWork(context);
        }
    }

    class MessageThread
    {
        public ulong LastMessage { get; set; }
        public string Content { get; set; }
    }

    private List<MessageThread> threads = new();
    private async Task DoGptWork(SocketCommandContext context)
    {
        var isAsking = await _gptService.IsAskingGarth(context.Message.Content);

        MessageThread thread = null;
        StringBuilder toAsk = new StringBuilder();
        if (!isAsking && context.Message.Reference != null && context.Message.ReferencedMessage.Author.Id == _discord.CurrentUser.Id)
        {
            thread = threads.FirstOrDefault(x => x.LastMessage == context.Message.Reference.MessageId.Value);
            if (thread != null)
            {
                isAsking = true;
                toAsk = new StringBuilder(thread.Content);
                toAsk.AppendLine($"{context.Message.Author.Username}: " + context.Message.Content);
            }
        } else if (isAsking)
            toAsk.AppendLine($"{context.Message.Author.Username}: " + context.Message.Content);
        
        if (isAsking)
        {
            using (var typing = context.Channel.EnterTypingState())
            {
                var reference = new MessageReference(context.Message.Id, context.Channel.Id, context.Guild.Id);
                var msg = await _gptService.GetResponse(toAsk.ToString().Trim() + "\nAI: ", context.Message.Author.Username);
                
                if (msg.Success)
                {
                    if(msg.Response.Length > 2000)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: new EmbedBuilder()
                                .WithColor(201, 62, 83)
                                .WithTitle("Error")
                                .WithDescription("Reply too long!")
                                .Build(),
                            messageReference: reference);
                        return;
                    }
                    
                    var reply = await context.Channel.SendMessageAsync(msg.Response.Replace("Garth: ", "").Replace("AI: ", ""), messageReference: reference);
                    thread ??= new MessageThread();
                    toAsk.AppendLine("AI: " + reply.Content.Trim());
                    thread.LastMessage = reply.Id;
                    thread.Content = toAsk.ToString();
                    if (!threads.Contains(thread))
                        threads.Add(thread);
                    
                    return;
                }

                await context.Channel.SendMessageAsync("",
                    embed: new EmbedBuilder()
                        .WithColor(201, 62, 83)
                        .WithTitle("Error")
                        .WithDescription(msg.Error)
                        .Build(),
                    messageReference: reference);
            }
        }
    }

    public async Task ReplyToInlineTags(SocketCommandContext context)
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

                if (tag.IsFile)
                {
                    await using MemoryStream stream = new MemoryStream(Convert.FromBase64String(tag.Content!));
                    await context.Channel.SendFileAsync(stream, tag.FileName);
                }
                else
                    await context.Channel.SendMessageAsync(tag.Content!);
            }
        }
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        // command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;

        // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
            return;

        // the command failed, let's notify the user that something happened.
        await context.Channel.SendMessageAsync($"error: {result}");
    }
}