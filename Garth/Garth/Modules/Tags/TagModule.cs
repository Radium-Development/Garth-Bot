using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules.Tags;

public class TagModule : GarthModuleBase
{
    [Command("tag")]
    [Alias("t")]
    public async Task Tag(string tagName)
    {
        var tag = await Context.TagDao.GetByName(tagName, Context.Guild.Id);

        if (tag is null)
            return;

        if (tag.IsFile)
        {
            await using MemoryStream stream = new(Convert.FromBase64String(tag.Content!));
            await Context.Channel.SendFileAsync(stream, tag.FileName);
            return;
        }

        await ReplyAsync(tag.Content);
    }
}