using SQLite;

namespace SleepJournal.Models;

public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Text { get; set; } = "";
    public int Mood { get; set; }
    public int SocialAnxiety { get; set; }
    public int Regretability { get; set; }
}