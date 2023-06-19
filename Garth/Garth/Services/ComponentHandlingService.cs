using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Garth.Components.Tags.Search;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.Helpers;
using Garth.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Renci.SshNet.Messages;
using IResult = Discord.Commands.IResult;

namespace Garth.Services;

public class ComponentHandlingService
{
    private readonly InteractionService _interactions;
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;
    private readonly Configuration.Config _configuration;
    private readonly GarthDbContext _db;

    public ComponentHandlingService(IServiceProvider services)
    {
        _interactions = services.GetRequiredService<InteractionService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _configuration = services.GetRequiredService<Configuration>();
        _db = services.GetRequiredService<GarthDbContext>();
        _services = services;

        _interactions.Log += Log;
        _discord.ButtonExecuted += InteractionHandlerAsync;
        _discord.SelectMenuExecuted += InteractionHandlerAsync;
        _discord.ModalSubmitted += ModalHandlerAsync;
    }
    
    private async Task Log(LogMessage arg)
    {
        Console.WriteLine(arg.Severity + " : " + arg.Exception);
    }

    public async Task InitializeAsync()
    {
        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }
    
    private async Task InteractionHandlerAsync(SocketMessageComponent component)
    {
        var context = new GarthInteractionContext(_discord, component, _db);
        var result = await _interactions.ExecuteCommandAsync(context, _services);
        if(!result.IsSuccess)
            await component.RespondAsync(embed: EmbedHelper.Error(result.ErrorReason));
    }
    
    private async Task ModalHandlerAsync(SocketModal modal)
    {
        var context = new GarthInteractionContext(_discord, modal, _db);
        var result = await _interactions.ExecuteCommandAsync(context, _services);
        if(!result.IsSuccess)
            await modal.RespondAsync(embed: EmbedHelper.Error(result.ErrorReason));
    }
}