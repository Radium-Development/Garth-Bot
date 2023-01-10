using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using Discord.Commands;

namespace Garth.Modules;

public class LMGTFYModule : ModuleBase<SocketCommandContext>
{
    [Command("lmgtfy"), Alias("google")]
    public async Task LMGTFY([Remainder, Optional]string? query)
    {
        query ??= Context.Message.ReferencedMessage.Content;

        await ReplyAsync($"https://letmegooglethat.com/?q={HttpUtility.UrlEncode(query)}");
    }
}