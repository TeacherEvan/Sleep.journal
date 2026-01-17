using SQLite;

namespace SleepJournal.Models;

/// <summary>
/// Represents a journal entry with emotional and psychological metrics.
/// </summary>
public class JournalEntry
{
    /// <summary>
    /// Gets or sets the unique identifier for the journal entry.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the entry was created.
    /// </summary>
    [Indexed(Name = "idx_journalentry_createdat", Order = 1)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the journal entry text (max 200 characters).
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Gets or sets the mood rating (1-10 scale).
    /// </summary>
    public int Mood { get; set; }

    /// <summary>
    /// Gets or sets the social anxiety level (1-10 scale).
    /// </summary>
    public int SocialAnxiety { get; set; }

    /// <summary>
    /// Gets or sets the regretability rating (1-10 scale).
    /// </summary>
    public int Regretability { get; set; }

    /// <summary>
    /// Gets the date portion of CreatedAt for display purposes.
    /// </summary>
    [Ignore]
    public DateTime EntryDate => CreatedAt.Date;
}