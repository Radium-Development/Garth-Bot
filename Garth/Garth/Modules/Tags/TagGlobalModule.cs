using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO.DAO;
using Garth.DAL.DAO.DomainClasses;

namespace Garth.Modules.Tags;

public class TagGlobalModule : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;
    private readonly TagDAO _tagDao;

    public TagGlobalModule(GarthDbContext context)
    {
        _db = context;
        _tagDao = new TagDAO(_db);
    }

    [Command("tag global")]
    [Alias("t global")]
    public async Task ToggleGlobal(string tagName)
    {
        var tag = await _tagDao.GetByName(tagName);

        if (tag == null)
        {
            await ReplyAsync($"Could not find the tag *{tagName}*");
            return;
        }

        tag.Global = !tag.Global;
        
        if (await _tagDao.Update(tag) == DBUpdateResult.Failed)
        {
            await ReplyAsync("Failed to update tag!");
            return;
        }
        
        await ReplyAsync($"Tag *{tag.Name}* is now **{(tag.Global ? "Global" : "Private")}**");
    }
}