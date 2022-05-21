using Discord.Commands;

namespace Garth.Modules;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    public async Task Ping()
    {
        await ReplyAsync("pong!");
    }
}