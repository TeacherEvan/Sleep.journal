using SleepJournal.Models;
using SQLite;
using Microsoft.Extensions.Logging;

namespace SleepJournal.Services;

/// <summary>
/// SQLite implementation of the data service for managing journal entries, user settings, and passages.
/// Implements thread-safe lazy initialization and Write-Ahead Logging (WAL) for optimal performance.
/// </summary>
public class SQLiteDataService : IDataService, IAsyncDisposable
{
    private readonly SQLiteAsyncConnection _db;
    private readonly ILogger<SQLiteDataService> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _isInitialized;
    private readonly string _dbPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="SQLiteDataService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for tracking database operations.</param>
    /// <param name="dbPath">Optional database file path. If null, uses platform-specific app data directory.</param>
    public SQLiteDataService(ILogger<SQLiteDataService> logger, string? dbPath = null)
    {
        _logger = logger;
        _dbPath = dbPath ?? Path.Combine(FileSystem.AppDataDirectory, "sleepjournal.db");
        _db = new SQLiteAsyncConnection(_dbPath);
        _logger.LogInformation("SQLiteDataService initialized with database path: {DbPath}", _dbPath);
    }

    /// <summary>
    /// Ensures the database is initialized with all tables and optimizations.
    /// Uses thread-safe lazy initialization pattern with SemaphoreSlim.
    /// Enables Write-Ahead Logging (WAL) mode for better performance and concurrency.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_isInitialized)
                return;

            // Create tables
            await _db.CreateTableAsync<JournalEntry>();
            await _db.CreateTableAsync<UserSettings>();
            await _db.CreateTableAsync<Passage>();

            // Enable WAL mode only for persistent databases, not temp test databases
            // WAL mode can cause issues with temp files on some systems
            if (!_dbPath.Contains(Path.GetTempPath()))
            {
                try
                {
                    await _db.ExecuteAsync("PRAGMA journal_mode = WAL");
                    await _db.ExecuteAsync("PRAGMA synchronous = NORMAL");
                    _logger.LogInformation("WAL mode enabled for database");
                }
                catch (Exception walEx)
                {
                    _logger.LogWarning(walEx, "Failed to enable WAL mode, continuing without it");
                }
            }

            // Create index on CreatedAt for faster ORDER BY queries
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_journalentry_createdat ON JournalEntry(CreatedAt DESC)");

            _isInitialized = true;
            _logger.LogInformation("Database initialized successfully with WAL mode and indexes");
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

    /// <summary>
    /// Saves a journal entry to the database.
    /// </summary>
    /// <param name="entry">The journal entry to save.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when entry is null.</exception>
    public async Task SaveEntryAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();

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

    /// <summary>
    /// Retrieves all journal entries ordered by creation date (most recent first).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of journal entries ordered by CreatedAt descending.</returns>
    public async Task<List<JournalEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

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

    /// <summary>
    /// Asynchronously disposes the database connection and releases resources.
    /// </summary>
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