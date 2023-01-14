using Garth.DAL.DomainClasses;
using Spectre.Console;

namespace Garth.DAL.DAO;

public class CommandHistoryDAO
{
    private readonly GarthDbContext _db;

    public CommandHistoryDAO(GarthDbContext ctx)
    {
        _db = ctx;
    }

    public async Task<DBUpdateResult> Add(CommandHistory commandHistory)
    {
        try
        {
            await _db.CommandHistory!.AddAsync(commandHistory);
            return (await _db.SaveChangesAsync()) > 0 ? DBUpdateResult.Sucess : DBUpdateResult.Failed;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return DBUpdateResult.Failed;
        }
    }
}