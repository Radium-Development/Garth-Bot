﻿using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules.Tags;

public class TagGlobalModule : GarthModuleBase
{
    [Command("tag global")]
    [Alias("t global")]
    public async Task ToggleGlobal(string tagName)
    {
        var tag = await Context.TagDao.GetByName(tagName, Context.Guild.Id);

        if (tag is null)
        {
            _ =  ReplyErrorAsync($"Could not find the tag *{tagName}*");
            return;
        }

        tag.Global = !tag.Global;
        
        if (await Context.TagDao.Update(tag) == DBUpdateResult.Failed)
        {
            _ =  ReplyErrorAsync("Failed to update tag!");
            return;
        }
        
        await ReplySuccessAsync($"Tag *{tag.Name}* is now **{(tag.Global ? "Global" : "Private")}**");
    }
}