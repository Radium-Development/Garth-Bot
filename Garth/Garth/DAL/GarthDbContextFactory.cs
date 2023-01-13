using Garth.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Helpers;

namespace Garth.DAL;

public class BloggingContextFactory : IDesignTimeDbContextFactory<GarthDbContext>
{
    public GarthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<GarthDbContext>();

        var sqlConnectionString = EnvironmentVariables.Get("GarthConnectionString", true)!;

        optionsBuilder.UseMySql(sqlConnectionString, ServerVersion.AutoDetect(sqlConnectionString));

        return new GarthDbContext(optionsBuilder.Options);
    }
}