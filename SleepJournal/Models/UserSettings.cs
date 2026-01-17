using SQLite;

namespace SleepJournal.Models;

/// <summary>
/// Represents application user settings stored as a singleton.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Gets or sets the singleton identifier (always 1).
    /// </summary>
    [PrimaryKey]
    public int Id { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether reminder notifications are enabled.
    /// </summary>
    public bool EnableReminders { get; set; }
}