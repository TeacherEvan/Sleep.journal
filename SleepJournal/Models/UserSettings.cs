using SQLite;

namespace SleepJournal.Models;

public class UserSettings
{
    [PrimaryKey]
    public int Id { get; set; } = 1; // Singleton
    public bool EnableReminders { get; set; }
}