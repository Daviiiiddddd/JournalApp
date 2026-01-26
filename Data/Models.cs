using SQLite;

namespace JournalApp.Data;

// Mood categories (analytics)
public enum MoodCategory
{
    Positive,
    Neutral,
    Negative
}

public static class MoodData
{
    // Based on coursework requirement list
    public static readonly string[] Positive =
    [
        "Happy", "Excited", "Relaxed", "Grateful", "Confident"
    ];

    public static readonly string[] Neutral =
    [
        "Calm", "Thoughtful", "Curious", "Nostalgic", "Bored"
    ];

    public static readonly string[] Negative =
    [
        "Sad", "Angry", "Stressed", "Lonely", "Anxious"
    ];

    public static MoodCategory GetCategory(string mood)
    {
        if (Positive.Contains(mood)) return MoodCategory.Positive;
        if (Negative.Contains(mood)) return MoodCategory.Negative;
        return MoodCategory.Neutral;
    }
}

// Main table: one journal entry per day
public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Enforced one-entry-per-day using unique index on date string (yyyy-MM-dd)
    [Indexed(Unique = true)]
    public string EntryDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

    // Title + content (Quill HTML or Markdown converted to HTML)
    public string Title { get; set; } = "Untitled";
    public string ContentHtml { get; set; } = ""; // rich text stored as HTML

    // Category
    public string Category { get; set; } = "General";

    // Mood tracking
    public string PrimaryMood { get; set; } = "Calm"; // required
    public string PrimaryMoodCategory { get; set; } = MoodCategory.Neutral.ToString(); // stored for analytics
    public string SecondaryMoodsCsv { get; set; } = ""; // up to 2 moods "Relaxed,Grateful"

    // Tags
    public string TagsCsv { get; set; } = ""; // "Work,Health,Travel"

    // System timestamps
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Analytics helper
    public int WordCount { get; set; } = 0;
}