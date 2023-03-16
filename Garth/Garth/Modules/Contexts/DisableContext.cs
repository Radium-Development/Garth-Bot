using Discord.Commands;
using Garth.DAL;

namespace Garth.Modules.Contexts;

public class DisableContext : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;

    public DisableContext(GarthDbContext context)
    {
        _db = context;
    }

    [Command("context disable"), Alias("ctx disable")]
    public async Task Tag(params int[] contextIds)
    {
        if (Context.Message.Author.Id != 201582886137233409)
        {
            _ = ReplyAsync("You cannot disable contexts!");
            return;
        }

        foreach (var contextId in contextIds)
        {
            var context = _db.Contexts!.FirstOrDefault(x => x.Id == contextId);
            
            if (context is null)
            {
                _ = ReplyAsync($"Could not find a context with ID: **{contextId}**");
                continue;
            }

            context.Enabled = false;
            _ = _db.Contexts!.Update(context);
            _ = ReplyAsync($"Disabled context with ID: **{contextId}**");
        }
        
        await _db.SaveChangesAsync();
    }
}