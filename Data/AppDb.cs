using SQLite;

namespace JournalApp.Data;

public class AppDb
{
    private SQLiteAsyncConnection? _conn;

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_conn != null)
            return _conn;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journalmate.db3");
        _conn = new SQLiteAsyncConnection(dbPath);

        // Create tables
        await _conn.CreateTableAsync<JournalEntry>();

        return _conn;
    }
}