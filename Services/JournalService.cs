using JournalApp.Data;
using JournalApp.Data.Repositories;

namespace JournalApp.Services;

public class JournalService
{
    private readonly JournalRepository _repo;

    public JournalService(JournalRepository repo)
    {
        _repo = repo;
    }

    // Get entry by date (calendar + editor)
    public Task<JournalEntry?> GetByDateAsync(DateTime date)
        => _repo.GetByDateAsync(date);

    // Save entry for a specific date (enforces 1/day in repo)
    public async Task SaveForDateAsync(
        DateTime date,
        string title,
        string contentHtml,
        string category,
        string primaryMood,
        List<string> secondaryMoods,
        List<string> tags)
    {
        // basic validation
        if (string.IsNullOrWhiteSpace(title)) title = "Untitled";
        if (string.IsNullOrWhiteSpace(category)) category = "General";
        if (string.IsNullOrWhiteSpace(primaryMood)) primaryMood = "Calm";

        // secondary moods: max 2 and not same as primary
        var secondary = secondaryMoods
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => !x.Equals(primaryMood, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .ToList();

        var cleanTags = tags
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // word count (simple)
        var wordCount = CountWordsFromHtml(contentHtml);

        var entry = new JournalEntry
        {
            EntryDate = date.ToString("yyyy-MM-dd"),
            Title = title,
            ContentHtml = contentHtml ?? "",
            Category = category,
            PrimaryMood = primaryMood,
            PrimaryMoodCategory = MoodData.GetCategory(primaryMood).ToString(),
            SecondaryMoodsCsv = string.Join(",", secondary),
            TagsCsv = string.Join(",", cleanTags),
            WordCount = wordCount
        };

        await _repo.UpsertAsync(entry);
    }

    public async Task DeleteAllAsync()
    {
        var all = await FilterAsync(DateTime.Today.AddYears(-50), DateTime.Today.AddYears(50), "", "", "");
        foreach (var e in all)
        {
            if (DateTime.TryParse(e.EntryDate, out var d))
            await DeleteByDateAsync(d.Date);
        }
    }

    public Task DeleteByDateAsync(DateTime date)
        => _repo.DeleteByDateAsync(date);

    public Task<List<JournalEntry>> GetRangeAsync(DateTime from, DateTime to)
        => _repo.GetRangeAsync(from, to);

    public Task<List<JournalEntry>> FilterAsync(DateTime from, DateTime to, string? search, string? mood, string? tag)
        => _repo.FilterAsync(from, to, search, mood, tag);

    public static int CountWordsFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return 0;
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
        var parts = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length;
    }

    private async Task SaveAsync(
    DateTime date,
    string title,
    string contentHtml,
    string mood,
    string moodCategory,
    string tagsCsv)
{
    var entry = new JournalEntry
    {
        EntryDate = date.ToString("yyyy-MM-dd"),
        Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
        ContentHtml = contentHtml ?? "",
        PrimaryMood = mood,
        PrimaryMoodCategory = moodCategory,
        TagsCsv = tagsCsv,
        WordCount = CountWordsFromHtml(contentHtml)
    };

    await _repo.UpsertAsync(entry);
}

    private async Task UpdateAsync(
        DateTime date,
        string title,
        string contentHtml,
        string mood,
        string moodCategory,
        string tagsCsv)
    {
        var existing = await GetByDateAsync(date);
        if (existing == null) return;

        existing.Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title;
        existing.ContentHtml = contentHtml ?? "";
        existing.PrimaryMood = mood;
        existing.PrimaryMoodCategory = moodCategory;
        existing.TagsCsv = tagsCsv;
        existing.WordCount = CountWordsFromHtml(contentHtml);
        await _repo.UpsertAsync(existing);
}

    public async Task UpsertAsync(DateTime date, string title, string content, string mood, string moodCategory, string tagsCsv)
    {
        // Convert plain text -> simple HTML (so Analytics & Export work)
        var safe = System.Net.WebUtility.HtmlEncode(content ?? "");
        var html = safe.Replace("\r\n", "\n").Replace("\n", "<br/>");

        var existing = await GetByDateAsync(date);

        if (existing == null)
        {
            // Create new entry
            await SaveAsync(date, title, html, mood, moodCategory, tagsCsv);
        }
        else
        {
            // Update existing entry
            await UpdateAsync(date, title, html, mood, moodCategory, tagsCsv);
        }
    }
}