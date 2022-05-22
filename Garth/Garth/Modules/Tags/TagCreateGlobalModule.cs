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
    public async Task CreateGlobal(string tagName, [Remainder]string content)
    {
        var result = await _tagDao.Add(new Tag()
        {  
            Name = tagName,
            Content = content,
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