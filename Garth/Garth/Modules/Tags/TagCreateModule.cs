using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;
using Garth.Helpers;

namespace Garth.Modules.Tags;

public class TagCreateModule : GarthModuleBase
{
    [Command("tag create")]
    [Alias("t create", "tag new", "t new")]
    public async Task Create(string tagName, [Remainder, Optional]string? content)
    {
        if(Regex.IsMatch(tagName, "[^A-Za-z0-9!.#@$%^&()]") || new string[] {"create", "delete", "info", "edit", "search", "add", "remove", "global", "createglobal", "addglobal"}.Contains(tagName.ToLower()))
        {
            _ =  ReplyErrorAsync("**Invalid tag name!**");
            return;
        }
        
        if(content == null && Context.Message.Attachments.Count == 0)
        {
            _ =  ReplyErrorAsync("**You need to supply a value for the tag!**");
            return;
        }
        
        if(Regex.IsMatch(tagName, "<@![0-9]{18}>"))
        {
            _ =  ReplyErrorAsync("**Illegal mention in tag name!**");
            return;
        }
        
        if (!string.IsNullOrWhiteSpace(content) && Regex.IsMatch(content, "<@![0-9]{18}>"))
        {
            _ =  ReplyErrorAsync("**Illegal mention in tag name!**");
            return;
        }

        if (await Context.TagDao.GetByName(tagName) is not null)
        {
            _ =  ReplyErrorAsync("**A tag with that name already exists**");
            return;
        }
        
        var tagContent = content;
        var fileName = string.Empty;
        bool isFile = false;
        if (Context.Message.Attachments.Count > 0)
        {
            isFile = true;
            WebClient wc = new();
            var bytes = await wc.DownloadDataTaskAsync(Context.Message.Attachments.FirstOrDefault()!.Url);
            tagContent = Convert.ToBase64String(bytes);
            fileName = Context.Message.Attachments.FirstOrDefault()!.Filename;
        }
        
        var result = await Context.TagDao.Add(new Tag()
        {  
            Name = tagName,
            Content = tagContent,
            IsFile = isFile,
            FileName = fileName,
            Server = Context.Guild.Id,
            CreatorId = Context.User.Id,
            CreatorName = Context.User.ToString()
        });
        
        if(result == DBUpdateResult.Sucess)
            _ = ReplySuccessAsync($"Created new tag: **{tagName}**");
        else
            _ = ReplyErrorAsync("Failed to create new tag!");
    }
}