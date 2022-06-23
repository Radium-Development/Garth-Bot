using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DomainClasses;

namespace Garth.Modules.Contexts;

public class EditContext : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;

    public EditContext(GarthDbContext context)
    {
        _db = context;
    }

    [Command("context edit")]
    public async Task Tag(int contextid, [Remainder]string contextValue)
    {
        if (Context.Message.Author.Id != 201582886137233409)
        {
            await ReplyAsync("You cannot create new contexts!");
            return;
        }
        
        Context contextTag = _db.Contexts!.FirstOrDefault(x => x.Id == contextid);

        if (contextTag == null)
        {
            await ReplyAsync("Context not found");
            return;
        }

        contextTag.Value = contextValue;
        _db.Contexts.Update(contextTag);
        await _db.SaveChangesAsync();

        await ReplyAsync("Changes saved!");
    }
}