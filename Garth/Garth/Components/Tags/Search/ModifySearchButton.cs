using Discord;
using Discord.Interactions;
using Garth.Helpers;
using Garth.Modules.Tags;

namespace Garth.Components.Tags.Search;

public class ModifySearchButton : GarthInteractionBase
{
    [ComponentInteraction("tags.search.modify.button", true)]
    public async Task Modify()
    {
        var sourceMessage = Context.Component.Message;
        var contextData = sourceMessage.Embeds.First().Footer!.Value.Text;
        var searchTerm = contextData.Split(":")[0];
        var page = int.Parse(contextData.Split(":")[1]);

        await Context.Interaction.RespondWithModalAsync<ModifySearchModal.SearchModal>("tags.search.modify.modal");
        await Context.Component.Message.DeleteAsync();
    }
}