using SQLite;

namespace JournalApp.Data.Repositories;

public class JournalRepository
{
    private readonly AppDb _db;

    public JournalRepository(AppDb db)
    {
        _db = db;
    }

    private async Task<SQLiteAsyncConnection> Conn()
        => await _db.GetConnectionAsync();

}