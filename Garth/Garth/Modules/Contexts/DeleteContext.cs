using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DomainClasses;

namespace Garth.Modules.Contexts;

public class DeleteContext : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;

    public DeleteContext(GarthDbContext context)
    {
        _db = context;
    }

    [Command("context delete")]
    public async Task Tag(int contextid)
    {
        if (Context.Message.Author.Id != 201582886137233409)
        {
            await ReplyAsync("You cannot delete contexts!");
            return;
        }

        _db.Contexts.Remove(_db.Contexts.FirstOrDefault(x => x.Id == contextid));
        await _db.SaveChangesAsync();
        
        await ReplyAsync("Deleted context with ID: **" + contextid + "**");
    }
}