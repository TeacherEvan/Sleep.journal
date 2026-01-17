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

            if (entry.Id == 0)
            {
                await _db.InsertAsync(entry);
                _logger.LogInformation("Journal entry created successfully. ID: {EntryId}, CreatedAt: {CreatedAt}", entry.Id, entry.CreatedAt);
            }
            else
            {
                await _db.UpdateAsync(entry);
                _logger.LogInformation("Journal entry updated successfully. ID: {EntryId}", entry.Id);
            }
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
    /// Retrieves all journal entries ordered by creation date (most recent first).
    /// Alias for GetEntriesAsync for consistency with new naming.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>List of journal entries ordered by CreatedAt descending.</returns>
    public Task<List<JournalEntry>> GetJournalEntriesAsync(CancellationToken cancellationToken = default)
        => GetEntriesAsync(cancellationToken);

    /// <summary>
    /// Retrieves a specific journal entry by ID.
    /// </summary>
    /// <param name="id">The entry ID.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The journal entry if found, null otherwise.</returns>
    public async Task<JournalEntry?> GetJournalEntryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await EnsureInitializedAsync(cancellationToken);
            var entry = await _db.Table<JournalEntry>().Where(e => e.Id == id).FirstOrDefaultAsync();

            if (entry == null)
            {
                _logger.LogWarning("Journal entry with ID {EntryId} not found", id);
            }
            else
            {
                _logger.LogInformation("Retrieved journal entry {EntryId}", id);
            }

            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve journal entry {EntryId}", id);
            throw;
        }
    }

    /// <summary>
    /// Deletes a journal entry by ID.
    /// </summary>
    /// <param name="id">The entry ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteJournalEntryAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await EnsureInitializedAsync(cancellationToken);
            await _db.DeleteAsync<JournalEntry>(id);
            _logger.LogInformation("Deleted journal entry {EntryId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete journal entry {EntryId}", id);
            throw;
        }
    }

    /// <summary>
    /// Retrieves user settings (singleton).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>User settings or null if none exist.</returns>
    public async Task<UserSettings?> GetUserSettingsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await EnsureInitializedAsync(cancellationToken);
            var settings = await _db.Table<UserSettings>().FirstOrDefaultAsync();
            _logger.LogInformation("Retrieved user settings");
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user settings");
            throw;
        }
    }

    /// <summary>
    /// Saves user settings (upsert operation).
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SaveUserSettingsAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await EnsureInitializedAsync(cancellationToken);

            // Ensure ID is always 1 (singleton)
            settings.Id = 1;

            var existing = await _db.Table<UserSettings>().FirstOrDefaultAsync();
            if (existing != null)
            {
                await _db.UpdateAsync(settings);
                _logger.LogInformation("Updated user settings");
            }
            else
            {
                await _db.InsertAsync(settings);
                _logger.LogInformation("Created user settings");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user settings");
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