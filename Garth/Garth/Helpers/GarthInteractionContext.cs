using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;

namespace Garth.Helpers;

public class GarthInteractionContext : SocketInteractionContext
{
    public GarthDbContext DbContext { get; }
    public TagDAO TagDao { get; }
    
    public SocketMessageComponent Component { get; }
    
    public SocketInteraction Interaction { get; }
    
    public GarthInteractionContext(DiscordSocketClient client, SocketMessageComponent component, GarthDbContext ctx) : base(client, component)
    {
        this.DbContext = ctx;
        this.TagDao = new TagDAO(ctx);
        this.Component = component;
        this.Interaction = component;
    }
    
    public GarthInteractionContext(DiscordSocketClient client, SocketInteraction interaction, GarthDbContext ctx) : base(client, interaction)
    {
        this.DbContext = ctx;
        this.TagDao = new TagDAO(ctx);
        this.Interaction = interaction;
    }
}