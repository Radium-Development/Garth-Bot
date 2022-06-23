using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules.Tags;

public class TagEditModule : GarthModuleBase
{
    [Command("tag edit")]
    [Alias("t edit")]
    public async Task Tag(string tagName, [Remainder]string content)
    {
        var tag = await Context.TagDao.GetByName(tagName, Context.Guild.Id);
        
        if (tag is null)
        {
            _ = ReplyErrorAsync("Tag not found!");
            return;
        }

        tag.Content = content;
        
        if (await Context.TagDao.Update(tag) == DBUpdateResult.Failed)
        {
            _ =  ReplyErrorAsync("Failed to update tag!");
            return;
        }
        
        await ReplySuccessAsync($"Updated **{tag.Name}**");
    }
}