﻿using Discord.Commands;
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

    [Command("context delete"), Alias("ctx delete")]
    public async Task Tag(params int[] contextIds)
    {
        if (Context.Message.Author.Id != 201582886137233409)
        {
            await ReplyAsync("You cannot delete contexts!");
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

            _ = _db.Contexts!.Remove(context);
            _ = ReplyAsync($"Deleted context with ID: **{contextId}**");
        }
        
        await _db.SaveChangesAsync();
    }
}