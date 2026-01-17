using SleepJournal.Models;
using SQLite;
using Microsoft.Extensions.Logging;

namespace SleepJournal.Services;

public class SQLiteDataService : IDataService, IAsyncDisposable
{
    private readonly SQLiteAsyncConnection _db;
    private readonly ILogger<SQLiteDataService> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;

    public SQLiteDataService(ILogger<SQLiteDataService> logger)
    {
        _logger = logger;
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "sleepjournal.db");
        _db = new SQLiteAsyncConnection(dbPath);
        _logger.LogInformation("SQLiteDataService initialized with database path: {DbPath}", dbPath);
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
                return;

            await _db.CreateTableAsync<JournalEntry>();
            await _db.CreateTableAsync<UserSettings>();
            await _db.CreateTableAsync<Passage>();

            _isInitialized = true;
            _logger.LogInformation("Database tables created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database tables");
            throw;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task SaveEntryAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        try
        {
            await EnsureInitializedAsync(cancellationToken);
            await _db.InsertAsync(entry);
            _logger.LogInformation("Journal entry saved successfully. ID: {EntryId}, CreatedAt: {CreatedAt}", entry.Id, entry.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save journal entry");
            throw;
        }
    }

    public async Task<List<JournalEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureInitializedAsync(cancellationToken);
            var entries = await _db.Table<JournalEntry>().OrderByDescending(e => e.CreatedAt).ToListAsync();
            _logger.LogInformation("Retrieved {Count} journal entries", entries.Count);
            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve journal entries");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_db != null)
        {
            await _db.CloseAsync();
            _logger.LogInformation("Database connection closed");
        }
        _initializationLock?.Dispose();
    }
}