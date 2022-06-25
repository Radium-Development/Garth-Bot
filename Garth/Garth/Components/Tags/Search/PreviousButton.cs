using Discord;
using Discord.Interactions;
using Garth.Helpers;
using Garth.Modules.Tags;

namespace Garth.Components.Tags.Search;

public class PreviousButton : GarthInteractionBase
{
    [ComponentInteraction("tags.search.previous.button", true)]
    public async Task Previous()
    {
        var sourceMessage = Context.Component.Message;
        var contextData = sourceMessage.Embeds.First().Footer!.Value.Text;
        var searchTerm = contextData.Split(":")[0];
        var page = int.Parse(contextData.Split(":")[1]);
        
        var response = await TagSearchModule.BuildResponse(Context.Client, searchTerm, Context.TagDao, Context.Guild.Id, --page);
        
        await Context.Component.UpdateAsync(x =>
            {
                x.Embed = response.Item1;
                x.Components = response.Item2;
            }
        );
    }
}