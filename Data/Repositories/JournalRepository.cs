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

    // =========================
    // READ (Single day)
    // =========================
    public async Task<JournalEntry?> GetByDateAsync(DateTime date)
    {
        var key = date.ToString("yyyy-MM-dd");
        var c = await Conn();

        return await c.Table<JournalEntry>()
            .Where(e => e.EntryDate == key)
            .FirstOrDefaultAsync();
    }

    // =========================
    // CREATE / UPDATE (Upsert)
    // One entry per day (EntryDate is unique)
    // =========================
    public async Task UpsertAsync(JournalEntry entry)
    {
        var c = await Conn();

        // Basic validation
        if (string.IsNullOrWhiteSpace(entry.EntryDate))
            entry.EntryDate = DateTime.Today.ToString("yyyy-MM-dd");

        if (string.IsNullOrWhiteSpace(entry.Title))
            entry.Title = "Untitled";

        entry.PrimaryMoodCategory = MoodData.GetCategory(entry.PrimaryMood).ToString();

        // Find existing entry for that day
        var existing = await c.Table<JournalEntry>()
            .Where(e => e.EntryDate == entry.EntryDate)
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            // New entry
            entry.CreatedAt = DateTime.Now;
            entry.UpdatedAt = DateTime.Now;

            await c.InsertAsync(entry);
        }
        else
        {
            // Update existing (keep CreatedAt)
            entry.Id = existing.Id;
            entry.CreatedAt = existing.CreatedAt;
            entry.UpdatedAt = DateTime.Now;

            await c.UpdateAsync(entry);
        }
    }

    // =========================
    // DELETE (By date)
    // =========================
    public async Task DeleteByDateAsync(DateTime date)
    {
        var c = await Conn();
        var key = date.ToString("yyyy-MM-dd");

        var existing = await c.Table<JournalEntry>()
            .Where(e => e.EntryDate == key)
            .FirstOrDefaultAsync();

        if (existing != null)
            await c.DeleteAsync(existing);
    }

    // =========================
    // READ (All)
    // =========================
    public async Task<List<JournalEntry>> GetAllAsync()
    {
        var c = await Conn();
        return await c.Table<JournalEntry>()
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    // =========================
    // READ (Range by date)
    // Used for export + analytics
    // =========================
    public async Task<List<JournalEntry>> GetRangeAsync(DateTime from, DateTime to)
{
    var c = await Conn();

    var f = from.Date.ToString("yyyy-MM-dd");
    var t = to.Date.ToString("yyyy-MM-dd");

    // Load all rows (journal apps typically small data; OK for coursework)
    var all = await c.Table<JournalEntry>()
        .OrderByDescending(e => e.EntryDate)
        .ToListAsync();

    // Filter in C# (safe; avoids SQLite function issues)
    return all
        .Where(e => string.Compare(e.EntryDate, f, StringComparison.Ordinal) >= 0
                 && string.Compare(e.EntryDate, t, StringComparison.Ordinal) <= 0)
        .OrderByDescending(e => e.EntryDate)
        .ToList();
}

    // =========================
    // SEARCH (Title + content)
    // =========================
    public async Task<List<JournalEntry>> SearchAsync(string query)
    {
        var c = await Conn();
        query = (query ?? "").Trim();

        if (string.IsNullOrWhiteSpace(query))
            return await GetAllAsync();

        return await c.Table<JournalEntry>()
            .Where(e => e.Title.Contains(query) || e.ContentHtml.Contains(query))
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    // =========================
    // FILTER (date range + optional mood + optional tag + optional search)
    // This directly supports Timeline filters
    // =========================
    public async Task<List<JournalEntry>> FilterAsync(
        DateTime from,
        DateTime to,
        string? search,
        string? mood,
        string? tag)
    {
        // Get date range first (fast & clean)
        var list = await GetRangeAsync(from, to);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            list = list.Where(e =>
                (e.Title ?? "").Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (e.ContentHtml ?? "").Contains(search, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(mood))
        {
            mood = mood.Trim();
            list = list.Where(e =>
                (e.PrimaryMood ?? "").Equals(mood, StringComparison.OrdinalIgnoreCase) ||
                (e.SecondaryMoodsCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Any(x => x.Trim().Equals(mood, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            tag = tag.Trim();
            list = list.Where(e =>
                (e.TagsCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Any(x => x.Trim().Equals(tag, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        return list.OrderByDescending(e => e.EntryDate).ToList();
    }
}