using Discord;
using Discord.Interactions;
using Garth.DAL.DAO;
using Garth.Helpers;

namespace Garth.Components.Tags.Edit;

public class DeleteButton : GarthInteractionBase
{
    [ComponentInteraction("tags.edit.delete.button", true)]
    public async Task next()
    {
        var originalResponse = Context.Component.Message;
        var tagName = originalResponse.Embeds.First().Title;
        var tag = await Context.TagDao.GetByName(tagName, Context.Guild.Id);

        await Context.TagDao.Remove(tag);

        await Context.Interaction.RespondAsync(embed: EmbedHelper.Success($"Deleted Tag: **{tag.Name}**"));
    }
}