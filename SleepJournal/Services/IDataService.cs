using SleepJournal.Models;

namespace SleepJournal.Services;

public interface IDataService
{
    Task SaveEntryAsync(JournalEntry entry, CancellationToken cancellationToken = default);
    Task<List<JournalEntry>> GetEntriesAsync(CancellationToken cancellationToken = default);
}