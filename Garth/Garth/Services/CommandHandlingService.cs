using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;
using Garth.IO;
using Microsoft.Extensions.DependencyInjection;
using IServiceProvider = System.IServiceProvider;

namespace Garth.Services;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;
    private readonly Configuration.Config _configuration;
    private readonly GarthDbContext _db;
    private readonly CommandHistoryDAO _commandHistoryDAO;
    private readonly EvalService _evalService;
    
    private readonly Regex emoteRegex = new Regex(@":\S*?garf\S*?:", RegexOptions.IgnoreCase);
    
    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _configuration = services.GetRequiredService<Configuration>();
        _db = services.GetRequiredService<GarthDbContext>();
        _evalService = services.GetRequiredService<EvalService>();
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
            _ = ReplyToInlineTags(context);
            return;
        }

        // Perform the execution of the command. In this method,
        // the command service will perform precondition and parsing check
        // then execute the command if one is matched.
        var cmdResult = await _commands.ExecuteAsync(context, argPos, _services);
        // Note that normally a result will be returned by this format, but here
        // we will handle the result in CommandExecutedAsync,
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