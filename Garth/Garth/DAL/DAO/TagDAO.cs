using Garth.DAL.DomainClasses;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Garth.DAL.DAO;

public class TagDAO
{
    private readonly GarthDbContext _db;
    
    public TagDAO(GarthDbContext ctx)
    {
        _db = ctx;
    }

    public async Task<DBUpdateResult> Add(Tag tag)
    {
        try
        {
            await _db.Tags!.AddAsync(tag);
            return (await _db.SaveChangesAsync()) > 0 ? DBUpdateResult.Sucess : DBUpdateResult.Failed;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return DBUpdateResult.Failed;
        }
    }

    public async Task<DBUpdateResult> Update(Tag tag)
    {
        _db.Tags!.Update(tag);
        return (await _db.SaveChangesAsync()) > 0 ? DBUpdateResult.Sucess : DBUpdateResult.Failed;
    }
    
    public async Task<List<Tag>> GetAll()
    {
        return await _db.Tags!.ToListAsync();
    }

    public async Task<Tag?> GetByName(string name)
    {
        return await _db.Tags!.FirstOrDefaultAsync(x => x.Name!.ToLower() == name.ToLower());
    }
    
    public async Task<Tag?> GetByName(string name, ulong server)
    {
        var tag = await _db.Tags!.FirstOrDefaultAsync(x => x.Name!.ToLower() == name.ToLower());
        return (tag is not null && (tag.Global || tag?.Server == server)) ? tag : null;
    }

    public async Task<DBUpdateResult> Remove(Tag tag)
    {
        _db.Tags!.Remove(tag);
        return (await _db.SaveChangesAsync()) > 0 ? DBUpdateResult.Sucess : DBUpdateResult.Failed;
    }
}