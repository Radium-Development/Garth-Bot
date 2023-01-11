using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Garth.Helpers;

namespace Garth.Modules;

public class LMGTFYModule : GarthModuleBase
{
    [Command("lmgtfy"), Alias("google", "help")]
    public async Task LMGTFY([Remainder]string? query = null)
    {
        query ??= Context.Message.ReferencedMessage.Content;

        MessageReference reference = CreateMessageReference(
            Context.Guild.Id,
            Context.Message.ReferencedMessage,
            Context.Message
        )!;

        string URL = $"https://letmegooglethat.com/?q={HttpUtility.UrlEncode(query)}";

        Embed response = new EmbedBuilder()
            .WithDescription($"**[{query}]({URL})**")
            .WithColor(Color.MaxDecimalValue)
            .Build();
        
        await ReplyAsync("", embed: response, messageReference: reference);
    }
}