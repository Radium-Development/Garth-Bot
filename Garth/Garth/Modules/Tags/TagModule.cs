using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO.DAO;
using Garth.DAL.DAO.DomainClasses;

namespace Garth.Modules.Tags;

public class TagModule : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;
    private readonly TagDAO _tagDao;

    public TagModule(GarthDbContext context)
    {
        _db = context;
        _tagDao = new TagDAO(_db);
    }
    
    [Command("tag")]
    [Alias("t")]
    public async Task Tag(string tagName)
    {
        var tag = await _tagDao.GetByName(tagName);
        await ReplyAsync(tag?.Content);
    }
}