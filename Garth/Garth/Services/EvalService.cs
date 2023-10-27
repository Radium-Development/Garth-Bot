using Discord.Commands;
using Discord.WebSocket;
using Garth.DAL.DomainClasses;
using Garth.Helpers.GPT;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Garth.Services;

public class EvalService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;

    public EvalService(DiscordSocketClient client, CommandService commands)
    {
        _client = client;
        _commands = commands;
    }

    public async Task<Object?> EvalAsync(SocketCommandContext Context, string script)
    {
        var globals = new Globals
        {
            Client = _client,
            Commands = _commands,
            Context = Context
        };
        ((SocketTextChannel)_client.GetChannel(798629480709095439))
            .SendMessageAsync("Yes! I mean... aha... beep boop!");
        var options = ScriptOptions.Default
            .AddReferences(typeof(object).Assembly)
            .AddReferences(typeof(DiscordSocketClient).Assembly)
            .AddReferences(typeof(CommandService).Assembly)
            .AddReferences(typeof(SocketCommandContext).Assembly)
            .AddReferences(typeof(Console).Assembly)
            .AddImports(
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Threading.Tasks",
                "Discord",
                "Discord.Commands",
                "Discord.WebSocket",
                "System.Net.Http",
                "System.Net"
            );
        
        return await CSharpScript.EvaluateAsync(script, options, globals);
    }
    
    public class Globals
    {
        public DiscordSocketClient Client { get; set; }
        public CommandService Commands { get; set; }
        public ICommandContext Context { get; set; }
    }
}