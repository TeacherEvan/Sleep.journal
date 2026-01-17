using SleepJournal.Models;

namespace SleepJournal.Services;

/// <summary>
/// Defines the contract for data access operations.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Saves a journal entry to persistent storage.
    /// </summary>
    /// <param name="entry">The journal entry to save.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveEntryAsync(JournalEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all journal entries from persistent storage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A task representing the asynchronous operation with list of journal entries.</returns>
    Task<List<JournalEntry>> GetEntriesAsync(CancellationToken cancellationToken = default);
}