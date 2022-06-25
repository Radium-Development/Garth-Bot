using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules.Tags;

public class TagSearchModule : GarthModuleBase
{
    [Command("tag search")]
    [Alias("t search")]
    public async Task Tag(string? searchTerm = null)
    {
        var response = await BuildResponse(Context.Client, searchTerm, Context.TagDao, Context.Guild.Id, 0);
        await ReplyAsync(embed: response.Item1, components: response.Item2);
    }
    
    public static async Task<(Embed, MessageComponent)> BuildResponse(IDiscordClient client, string? searchTerm, TagDAO TagDao, ulong Guild, int page)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            searchTerm = null;
        
        var tags = await TagDao.Search(searchTerm, Guild, skip: 10 * page);

        StringBuilder sb = new("```");
        
        int index = 1;
        foreach (var tag in tags)
            sb.AppendLine($"{((10 * page) + index++).ToString().PadLeft(3, ' ')}: {tag.Name}");

        if (tags.Count == 0)
            sb.AppendLine("No results!");
        
        sb.AppendLine("```");

        var builder = new ComponentBuilder()
            .WithButton("Previous", "tags.search.previous.button", disabled: page <= 0)
            .WithButton("Select Tag", "tags.search.select.button")
            .WithButton("Modify Search", "tags.search.modify.button")
            .WithButton("Next", "tags.search.next.button", disabled: (await TagDao.Search(searchTerm, Guild, skip: 10 * (page + 1))).Count == 0);
        
        var embed = new EmbedHelper()
            .WithDescription(sb.ToString())
            .WithTitle(searchTerm == null ? "Searching all tags" : $"Searching for tags containing: {searchTerm}")
            .AsInfo()
            .WithFooter($"{searchTerm}:{page}")
            .Build();

        return (embed, builder.Build());
    }
}