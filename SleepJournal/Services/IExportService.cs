namespace SleepJournal.Services;

/// <summary>
/// Service for exporting journal entries to external formats.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports all journal entries to CSV (RFC 4180) and writes them to the
    /// caller-provided stream. Caller owns the stream and is responsible for
    /// disposing it.
    /// </summary>
    /// <param name="output">Destination stream (writable, not closed by this method).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ExportToCsvAsync(Stream output, CancellationToken cancellationToken = default);
}
