using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO.DAO;
using Garth.DAL.DAO.DomainClasses;

namespace Garth.Modules.Tags;

public class TagCreateModuleGlobal : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;
    private readonly TagDAO _tagDao;

    public TagCreateModuleGlobal(GarthDbContext context)
    {
        _db = context;
        _tagDao = new TagDAO(_db);
    }
    
    [Command("tag createglobal")]
    [Alias("t createglobal", "tag newglobal", "t newglobal")]
    public async Task CreateGlobal(string tagName, [Remainder, Optional]string? content)
    {
        if(Regex.IsMatch(tagName, "[^A-Za-z0-9!.#@$%^&()]") || new string[] {"create", "delete", "info", "edit", "search", "add", "remove", "global", "createglobal", "addglobal"}.Contains(tagName.ToLower()))
        {
            await ReplyAsync("**Invalid tag name!**");
            return;
        }
        
        if(content == null && Context.Message.Attachments.Count == 0)
        {
            await ReplyAsync("**You need to supply a value for the tag!**");
            return;
        }
        
        if(Regex.IsMatch(tagName, "<@![0-9]{18}>"))
        {
            await ReplyAsync("**Illegal mention in tag name!**");
            return;
        }
        
        if (!string.IsNullOrWhiteSpace(content) && Regex.IsMatch(content, "<@![0-9]{18}>"))
        {
            await ReplyAsync("**Illegal mention in tag name!**");
            return;
        }

        if (await _tagDao.GetByName(tagName) != null)
        {
            await ReplyAsync("**A tag with that name already exists!");
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
            fileName = Context.Message.Attachments.FirstOrDefault()!.Filename;
        }
        
        var result = await _tagDao.Add(new Tag()
        {  
            Name = tagName,
            Content = tagContent,
            IsFile = isFile,
            FileName = fileName,
            Server = Context.Guild.Id,
            CreatorId = Context.User.Id,
            CreatorName = Context.User.ToString(),
            Global = true
        });
        
        if(result == DBUpdateResult.Sucess)
            await ReplyAsync($"Created new tag: **{tagName}**");
        else
            await ReplyAsync("Failed to create new tag!");
    }
}