using Discord.Interactions;
using Garth.Helpers;
using Garth.Modules.Tags;

namespace Garth.Components.Tags.Edit;

public class ModifyModal : GarthInteractionBase
{
    
    [ModalInteraction("tags.edit.modify.modal", true)]
    public async Task Modify(SearchModal modal)
    {
        var response = await TagSearchModule.BuildResponse(Context.Client, modal.Value, Context.TagDao, Context.Guild.Id, 0);

        await Context.Interaction.RespondAsync(embed: response.Item1, components: response.Item2);
    }

    public class SearchModal : IModal
    {
        [InputLabel("New Value")]
        [ModalTextInput("value", placeholder: "New Value", minLength: 1)]
        public string? Value { get; set; }

        public string Title => "Modify Tag";
    }
}