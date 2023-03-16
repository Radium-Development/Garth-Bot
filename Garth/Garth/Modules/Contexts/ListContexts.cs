using System.Text;
using Discord.Commands;
using Garth.DAL;
using Garth.DAL.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace Garth.Modules.Contexts;

public class ListContexts : ModuleBase<SocketCommandContext>
{
    private readonly GarthDbContext _db;

    public ListContexts(GarthDbContext context)
    {
        _db = context;
    }

    [Command("context list"), Alias("ctx list")]
    public async Task Tag()
    {
        if (Context.Message.Author.Id != 201582886137233409)
        {
            _ = ReplyAsync("You cannot list contexts!");
            return;
        }

        StringBuilder sb = new("```basic\n ID | E | Value\n");
        
        List<Context> contexts = await _db.Contexts!.ToListAsync();
        foreach (var context in contexts)
            sb.AppendLine($"{context.Id.ToString().PadLeft(3, '0')} | {(context.Enabled ? "T" : "F")} | \"{context.Value}\"");
        sb.AppendLine("```");

        await ReplyAsync(sb.ToString());
    }
}