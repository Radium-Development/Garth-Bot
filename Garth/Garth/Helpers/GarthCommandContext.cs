using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;

namespace Garth.Helpers;

public class GarthCommandContext : SocketCommandContext
{
    public GarthDbContext DbContext { get; }
    public TagDAO TagDao { get; }
    
    public SocketMessageComponent Component { get; }
    
    public GarthCommandContext(DiscordSocketClient client, SocketUserMessage msg, GarthDbContext ctx) : base(client, msg)
    {
        this.DbContext = ctx;
        this.TagDao = new TagDAO(ctx);
    }
}