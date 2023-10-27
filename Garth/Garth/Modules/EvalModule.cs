using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Threading.Tasks;
using Garth.Services;

namespace Garth.Modules;

public class EvalModule : ModuleBase<SocketCommandContext>
{
    private readonly EvalService _evalService;

    public EvalModule(EvalService evalService)
    {
        _evalService = evalService;
    }

    [Command("eval")]
    [Summary("Evaluates C# code.")]
    public async Task EvalAsync([Remainder][Summary("The script to evaluate")] string script)
    {
        // Eric
        if (Context.User.Id == 166730885897388032) return;
        
        try 
        {
            var result = await _evalService.EvalAsync(Context, script);
            await ReplyAsync(result.ToString());
        }
        catch (Exception e)
        {
            await ReplyAsync($"An error occurred: {e.Message}");
        }
    }
}