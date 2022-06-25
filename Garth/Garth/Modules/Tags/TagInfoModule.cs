using System.Text;
using Arch.EntityFrameworkCore.Internal;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules.Tags;

public class TagInfoModule : GarthModuleBase
{
    [Command("tag info")]
    [Alias("t info")]
    public async Task Tag(string tagName)
    {
        var tag = await Context.TagDao.GetByName(tagName, Context.Guild.Id);

        var response = await BuildResponse(Context.Client, tag);
        await ReplyAsync(embed: response.Item1, components: response.Item2);
    }

    public static async Task<(Embed, MessageComponent)> BuildResponse(IDiscordClient client, Tag tag)
    {
        var creator = await client.GetUserAsync(tag.CreatorId);

        string type = tag.Content.StartsWith("http") ? "Link" : "Text";
        if (tag.IsFile || tag.Content.StartsWith("http"))
        {
            string ext = tag.Content.StartsWith("http") ? Path.GetExtension(tag.Content) : Path.GetExtension(tag.FileName);
            type = (ext.ToLower()) switch
            {
                _ when new[] { ".mp4", ".mov", ".wmv", ".avi", ".mkv", ".m4v" }.Contains(ext) => "Video",
                _ when new[] { ".png", ".jpg", ".jpeg", ".webm" }.Contains(ext) => "Image",
                _ when new[] { ".mp3", ".wav", ".ogg" }.Contains(ext) => "Audio",
                _ when new[] { ".gif", ".gifv" }.Contains(ext) => "Gif",
                _ => "File"
            };
        }
        
        Embed embed = new EmbedHelper()
            .WithDescription(tag.IsFile ? "" : (tag.Content.StartsWith("http") ? $"[{tag.Content}]({tag.Content})" :$"```{tag.Content}```"))
            .AsInfo()
            .WithTitle(tag.Name)
            .WithImageUrl(type == "Image" ? tag.Content : "")
            .AddField("Tag Type", type, inline: true)
            .AddField("Is Global?", tag.Global ? "Yes" : "No", inline: true)
            .AddField("Server ID", tag.Server, inline: true)
            .AddField("File Name", tag.IsFile ? tag.FileName : "N/A", inline: true)
            .WithTimestamp(tag.CreationDate.Value)
            .WithAuthor(creator)
            .Build();

        ComponentBuilder componentBuilder = new ComponentBuilder()
            .WithButton("Modify", "tags.edit.modify.button", disabled: true)
            .WithButton("Close", "tags.edit.close.button", ButtonStyle.Secondary)
            .WithButton("Delete", "tags.edit.delete.button", ButtonStyle.Danger);

        return (embed, componentBuilder.Build());
    }
}