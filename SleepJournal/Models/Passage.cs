using SQLite;

namespace SleepJournal.Models;

public class Passage
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
}