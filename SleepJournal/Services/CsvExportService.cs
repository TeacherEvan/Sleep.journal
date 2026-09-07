using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using SleepJournal.Models;

namespace SleepJournal.Services;

/// <summary>
/// CSV exporter for journal entries. RFC 4180 compliant: CRLF line endings,
/// fields containing comma/quote/CR/LF are wrapped in double-quotes with
/// embedded quotes doubled.
/// </summary>
public class CsvExportService : IExportService
{
    private const string Header = "Id,CreatedAt,Mood,SocialAnxiety,Regretability,Text";
    private const string LineEnding = "\r\n";

    private readonly IDataService _dataService;
    private readonly ILogger<CsvExportService> _logger;

    public CsvExportService(IDataService dataService, ILogger<CsvExportService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExportToCsvAsync(Stream output, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (!output.CanWrite)
        {
            throw new ArgumentException("Stream must be writable.", nameof(output));
        }

        // LeaveNewOpen=true: do not close the caller's stream on dispose.
        await using var writer = new StreamWriter(output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true);

        try
        {
            var entries = await _dataService.GetEntriesAsync(cancellationToken).ConfigureAwait(false);

            await writer.WriteAsync(Header).ConfigureAwait(false);
            await writer.WriteAsync(LineEnding).ConfigureAwait(false);

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await WriteRowAsync(writer, entry).ConfigureAwait(false);
            }

            await writer.FlushAsync().ConfigureAwait(false);
            _logger.LogInformation("Exported {Count} entries to CSV", entries.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CSV export failed");
            throw;
        }
    }

    private static async Task WriteRowAsync(StreamWriter writer, JournalEntry entry)
    {
        var row = string.Join(',', new[]
        {
            entry.Id.ToString(CultureInfo.InvariantCulture),
            entry.CreatedAt.ToString("O", CultureInfo.InvariantCulture),
            entry.Mood.ToString(CultureInfo.InvariantCulture),
            entry.SocialAnxiety.ToString(CultureInfo.InvariantCulture),
            entry.Regretability.ToString(CultureInfo.InvariantCulture),
            EscapeField(entry.Text),
        });
        await writer.WriteAsync(row).ConfigureAwait(false);
        await writer.WriteAsync(LineEnding).ConfigureAwait(false);
    }

    /// <summary>
    /// Convenience overload: writes the CSV to <paramref name="filePath"/>.
    /// Creates the parent directory if missing; overwrites any existing file.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is null or empty.
    /// </exception>
    public async Task ExportToFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // FileMode.Create = overwrite if exists. LeaveOpen=false so we dispose the stream.
        await using var stream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        await ExportToCsvAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// RFC 4180 field escaping. Wraps in double-quotes and doubles any embedded
    /// double-quote when the field contains comma, quote, CR, or LF.
    /// </summary>
    public static string EscapeField(string value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var mustQuote = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        if (!mustQuote)
        {
            return value;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
