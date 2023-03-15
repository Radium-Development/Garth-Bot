using Discord.Commands;
using Garth.DAL;

namespace Garth.Modules.Contexts;

public class EnableContext : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;

    public EnableContext(GarthDbContext context)
    {
        _db = context;
    }

    [Command("context enable"), Alias("ctx enable")]
    public async Task Tag(params int[] contextIds)
    {
        if (Context.Message.Author.Id != 201582886137233409)
        {
            _ = ReplyAsync("You cannot enable contexts!");
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

            context.Enabled = true;
            _ = _db.Contexts!.Update(context);
            _ = ReplyAsync($"Enabled context with ID: **{contextId}**");
        }
        
        await _db.SaveChangesAsync();
    }
}