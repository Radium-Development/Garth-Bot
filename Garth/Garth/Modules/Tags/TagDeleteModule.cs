using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules.Tags;

public class TagDeleteModule : GarthModuleBase
{
    [Command("tag delete")]
    [Alias("t delete")]
    public async Task Tag(string tagName)
    {
        var tag = await Context.TagDao.GetByName(tagName, Context.Guild.Id);
        
        if (tag is null)
        {
            _ = ReplyErrorAsync("Tag not found!");
            return;
        }
        
        if (await Context.TagDao.Remove(tag) == DBUpdateResult.Failed)
        {
            _ =  ReplyErrorAsync("Failed to delete tag!");
            return;
        }
        
        await ReplySuccessAsync($"Deleted **{tag.Name}**");
    }
}