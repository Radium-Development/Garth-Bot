using Discord.Commands;
using Garth.Helpers;

namespace Garth.Modules;

public class PingModule : GarthModuleBase
{
    [Command("ping")]
    public async Task Ping()
    {
        await ReplyAsync("pong!");
    }
}