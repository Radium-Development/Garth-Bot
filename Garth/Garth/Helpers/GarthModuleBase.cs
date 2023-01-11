using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Garth.Helpers;

public class GarthModuleBase : ModuleBase<GarthCommandContext>
{
    public async Task ReplyEmbedAsync(Embed embed) =>
        await this.ReplyAsync(embed: embed);

    public async Task ReplyErrorAsync(string msg, string title = "Error")
        => await ReplyEmbedAsync(EmbedHelper.Error(msg, title));
    
    public async Task ReplySuccessAsync(string msg, string title = "Success")
        => await ReplyEmbedAsync(EmbedHelper.Success(msg, title));
    
    public async Task ReplyWarningAsync(string msg, string title = "Warning")
        => await ReplyEmbedAsync(EmbedHelper.Warning(msg, title));

    public MessageReference? CreateMessageReference(ulong guild, params IMessage?[] messages)
    {
        foreach (IMessage? message in messages)
        {
            if (message is null)
                continue;

            return new MessageReference(message.Id, message.Channel.Id, guild, false);
        }

        return null;
    }
}