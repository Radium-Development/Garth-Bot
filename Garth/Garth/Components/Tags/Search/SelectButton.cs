using Discord;
using Discord.Interactions;
using Garth.Helpers;

namespace Garth.Components.Tags.Search;

public class SelectButton : GarthInteractionBase
{
    [ComponentInteraction("tags.search.select.button", true)]
    public async Task Select()
    {
        var sourceMessage = Context.Component.Message;
        var contextData = sourceMessage.Embeds.First().Footer!.Value.Text;
        var searchTerm = contextData.Split(":")[0];
        var page = int.Parse(contextData.Split(":")[1]);
        
        var tags = await Context.TagDao.Search(searchTerm, Context.Guild.Id, skip: 10 * page);
        
        var selectionBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Select a tag")
            .WithCustomId("tags.search.select.dropdown");

        int index = 1;
        foreach (var tag in tags)
        {
            string type = "Text";
            if (tag.IsFile)
            {
                string ext = Path.GetExtension(Path.GetExtension(tag.FileName));
                type = (ext.ToLower()) switch
                {
                    _ when new[] { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".m4v" }.Contains(ext) => "Video",
                    _ when new[] { ".png", ".jpg", ".jpeg", ".webm" }.Contains(ext) => "Image",
                    _ when new[] { ".mp3", ".wav", ".ogg" }.Contains(ext) => "Audio",
                    _ when new[] { ".gif", ".gifv" }.Contains(ext) => "Gif",
                    _ => "File"
                };
            }
            selectionBuilder.AddOption(tag.Name, tag.Name, $"Created {String.Format("{0:dddd, MMMM d, yyyy}", tag.CreationDate!)}  |  {type}");
        }

        await Context.Component.UpdateAsync(x =>
            {
                x.Content = "";
                x.Components = new ComponentBuilder()
                    .WithSelectMenu(selectionBuilder)
                    .Build();
            }
        );
    }
}