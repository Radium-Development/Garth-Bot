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

    public async Task<List<Tag>> GetAll()
    {
        return await _db.Tags!.ToListAsync();
    }

    public async Task<Tag?> GetByName(string name)
    {
        return await _db.Tags!.FirstOrDefaultAsync(x => x.Name!.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}