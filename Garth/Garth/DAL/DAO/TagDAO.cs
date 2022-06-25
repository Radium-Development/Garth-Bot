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
    
    public async Task<List<Tag>> GetAll(ulong? guildid = null)
    {
        return await _db.Tags!.Where(x => guildid == null || x.Global || x.Server == guildid).OrderBy(x => x.Name).ToListAsync();
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

    public async Task<List<Tag>> Search(string? searchTerm = null, ulong? guildid = null, int skip = 0, int count = 10)
    {
        if (searchTerm is null)
            return (await GetAll(guildid)).Skip(skip).Take(count).ToList();
        
        var tags = await _db.Tags!
            .Where(x => guildid == null || x.Global || x.Server == guildid)
            .Where(x => x.Name.ToLower().Contains(searchTerm.ToLower()))
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(count)
            .ToListAsync();
        return tags;
    }
}