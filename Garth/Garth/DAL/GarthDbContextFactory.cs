using Garth.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Garth.DAL;

public class BloggingContextFactory : IDesignTimeDbContextFactory<GarthDbContext>
{
    public GarthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<GarthDbContext>();

        var sqlConnectionString = Environment.GetEnvironmentVariable("GarthConnectionString", EnvironmentVariableTarget.Process);
        sqlConnectionString ??= Environment.GetEnvironmentVariable("GarthConnectionString", EnvironmentVariableTarget.User);
        sqlConnectionString ??= Environment.GetEnvironmentVariable("GarthConnectionString", EnvironmentVariableTarget.Machine);
        if (sqlConnectionString is null)
            throw new Exception("Environment variable 'GarthConnectionString' is not set!");

        optionsBuilder.UseMySql(sqlConnectionString, ServerVersion.AutoDetect(sqlConnectionString));

        return new GarthDbContext(optionsBuilder.Options);
    }
}