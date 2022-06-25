using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Garth.DAL.DomainClasses;
using Garth.Helpers;
using Garth.Modules.Tags;
using Renci.SshNet.Messages;

namespace Garth.Components.Tags.Search;

public class ModifySearchModal : GarthInteractionBase
{
    [ModalInteraction("tags.search.modify.modal", true)]
    public async Task Modify(SearchModal modal)
    {
        var response = await TagSearchModule.BuildResponse(Context.Client, modal.Value, Context.TagDao, Context.Guild.Id, 0);

        await Context.Interaction.RespondAsync(embed: response.Item1, components: response.Item2);
    }

    public class SearchModal : IModal
    {
        [InputLabel("Search Term")]
        [ModalTextInput("value", placeholder: "pepe", minLength: 0)]
        public string? Value { get; set; }

        public string Title => "Modify Search";
    }
}