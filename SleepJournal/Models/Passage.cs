using SQLite;

namespace SleepJournal.Models;

/// <summary>
/// Represents a passage for future content management features.
/// Currently unused but reserved for expansion.
/// </summary>
public class Passage
{
    /// <summary>
    /// Gets or sets the unique identifier for the passage.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the passage title.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the passage content.
    /// </summary>
    public string Content { get; set; } = "";
}