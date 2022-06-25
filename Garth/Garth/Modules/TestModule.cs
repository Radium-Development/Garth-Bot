using Discord;
using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules;

public class TestModule : GarthModuleBase
{
    [Command("test")]
    public async Task Tag()
    {
        var builder = new ComponentBuilder()
            .WithButton("test", "tag.search.test");

        var embedBuilder = EmbedHelper.Success("Test Message");
        
        await ReplyAsync("", embed: embedBuilder, components: builder.Build());
    }
}