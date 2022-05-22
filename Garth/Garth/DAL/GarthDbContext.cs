using Microsoft.EntityFrameworkCore;
using Garth.DAL.DAO.DomainClasses;

namespace Garth.DAL;

public class GarthDbContext : DbContext
{
    public GarthDbContext(DbContextOptions<GarthDbContext> options) : base(options) { }

    public virtual DbSet<Tag>? Tags { get; set; }
}