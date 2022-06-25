using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Garth.Helpers;
using Garth.Modules.Tags;

namespace Garth.Components.Tags.Search;

public class SelectDropdown : GarthInteractionBase
{
    [ComponentInteraction("tags.search.select.dropdown", true)]
    public async Task Select()
    {
        var tag = await Context.TagDao.GetByName(Context.Component.Data.Values.First(), Context.Guild.Id);
        var response = await TagInfoModule.BuildResponse(Context.Client, tag);
        await Context.Component.UpdateAsync(async x =>
        {
            x.Embed = response.Item1;
            x.Components = response.Item2;
        });
    }
}