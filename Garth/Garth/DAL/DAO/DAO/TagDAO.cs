using Garth.DAL.DAO.DomainClasses;
using Microsoft.EntityFrameworkCore;

namespace Garth.DAL.DAO.DAO;

public class TagDAO
{
    private readonly GarthDbContext _db;
    
    public TagDAO(GarthDbContext ctx)
    {
        _db = ctx;
    }

    public async Task<DBUpdateResult> Add(Tag tag)
    {
        await _db.Tags!.AddAsync(tag);
        return (await _db.SaveChangesAsync()) > 0 ? DBUpdateResult.Sucess : DBUpdateResult.Failed;
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
        return (tag.Global || tag?.Server == server) ? tag : null;
    }
}