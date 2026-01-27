using JournalApp.Data;
using JournalApp.Data.Repositories;

namespace JournalApp.Services;

public record StreakInfo(int CurrentStreak, int LongestStreak, int MissedDays);

public class AnalyticsService
{
    private readonly JournalRepository _repo;

    public AnalyticsService(JournalRepository repo)
    {
        _repo = repo;
    }

    // -------- Streaks (based on EntryDate presence) --------
    public async Task<StreakInfo> GetStreakInfoAsync(DateTime from, DateTime to)
    {
        var entries = await _repo.GetRangeAsync(from, to);
        var dates = entries.Select(e => e.EntryDate).ToHashSet();

        // Current streak up to today
        int current = 0;
        var d = DateTime.Today;

        while (dates.Contains(d.ToString("yyyy-MM-dd")))
        {
            current++;
            d = d.AddDays(-1);
        }

        // Longest streak in the selected range + missed days
        var allDays = Enumerable.Range(0, (to.Date - from.Date).Days + 1)
            .Select(i => from.Date.AddDays(i));

        int longest = 0;
        int run = 0;
        int missed = 0;

        foreach (var day in allDays)
        {
            if (dates.Contains(day.ToString("yyyy-MM-dd")))
            {
                run++;
                longest = Math.Max(longest, run);
            }
            else
            {
                missed++;
                run = 0;
            }
        }

        return new StreakInfo(current, longest, missed);
    }

    // -------- Mood Distribution (Positive/Neutral/Negative) --------
    public async Task<Dictionary<string, int>> MoodDistributionAsync(DateTime from, DateTime to)
    {
        var entries = await _repo.GetRangeAsync(from, to);
        return entries
            .GroupBy(e => e.PrimaryMoodCategory)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // -------- Most Frequent Moods --------
    public async Task<Dictionary<string, int>> TopMoodsAsync(DateTime from, DateTime to, int take = 5)
    {
        var entries = await _repo.GetRangeAsync(from, to);
        return entries
            .GroupBy(e => e.PrimaryMood)
            .OrderByDescending(g => g.Count())
            .Take(take)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // -------- Most Used Tags --------
    public async Task<Dictionary<string, int>> TopTagsAsync(DateTime from, DateTime to, int take = 10)
    {
        var entries = await _repo.GetRangeAsync(from, to);

        var tags = new List<string>();
        foreach (var e in entries)
        {
            tags.AddRange((e.TagsCsv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim()));
        }

        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(take)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    // -------- Tag Breakdown (% of entries that include each tag) --------
    public async Task<Dictionary<string, double>> TagBreakdownPercentAsync(DateTime from, DateTime to, int take = 8)
    {
        var entries = await _repo.GetRangeAsync(from, to);
        if (entries.Count == 0) return new Dictionary<string, double>();

        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in entries)
        {
            var uniqueTags = (e.TagsCsv ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var t in uniqueTags)
            {
                tagCounts[t] = tagCounts.GetValueOrDefault(t) + 1;
            }
        }

        return tagCounts
            .OrderByDescending(kv => kv.Value)
            .Take(take)
            .ToDictionary(kv => kv.Key, kv => (kv.Value * 100.0) / entries.Count);
    }

    // -------- Word Count Trend (date -> words) --------
    public async Task<List<(string Date, int Words)>> WordTrendAsync(DateTime from, DateTime to)
    {
        var entries = await _repo.GetRangeAsync(from, to);
        return entries
            .OrderBy(e => e.EntryDate)
            .Select(e => (e.EntryDate, e.WordCount))
            .ToList();
    }
}