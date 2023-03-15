using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DAO;
using Garth.DAL.DomainClasses;

namespace Garth.Modules.Contexts;

public class AddContext : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;

    public AddContext(GarthDbContext context)
    {
        _db = context;
    }

    [Command("context add"), Alias("ctx add")]
    public async Task Tag([Remainder]string contextValue)
    {
        if (Context.Message.Author.Id != 201582886137233409)
        {
            await ReplyAsync("You cannot create new contexts!");
            return;
        }
        
        var newContext = _db.Contexts!.Add(new Context()
        {
            Value = contextValue,
            CreatorId = Context.Message.Author.Id
        });
        await _db.SaveChangesAsync();
        await ReplyAsync("Created new context width ID: **" + newContext.Entity.Id + "**");
    }
}