using Discord;
using Discord.Interactions;
using Garth.Helpers;

namespace Garth.Components.Tags.Edit;

public class ModifyButton : GarthInteractionBase
{
    [ModalInteraction("tags.edit.close.button")]
    public async Task Close()
    {
        await Context.Component.Message.DeleteAsync();
    }
    
    [ComponentInteraction("tags.edit.modify.button", true)]
    public async Task next()
    {
        var originalResponse = Context.Component.Message;
        var tagName = originalResponse.Embeds.First().Title;
        var tag = await Context.TagDao.GetByName(tagName, Context.Guild.Id);

        Modal modal = new ModalBuilder()
            .WithTitle($"Modify: {tag.Name}")
            .WithCustomId("tags.edit.modify.modal")
            .AddTextInput("Value", "value", value: tag.Content, placeholder: "Hello, I am Mr. Potato", style: TextInputStyle.Paragraph)
            .Build();

        await Context.Interaction.RespondWithModalAsync<ModifyModal.SearchModal>("tags.edit.modify.modal", modifyModal: x => x.WithTitle("Modify Tag: " + tag.Name));
    }
}