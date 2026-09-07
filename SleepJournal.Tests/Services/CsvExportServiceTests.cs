using SleepJournal.Models;
using SleepJournal.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace SleepJournal.Tests.Services;

/// <summary>
/// Unit tests for CsvExportService. Covers RFC 4180 escaping and edge cases.
/// </summary>
public class CsvExportServiceTests
{
    private const string ExpectedHeader = "Id,CreatedAt,Mood,SocialAnxiety,Regretability,Text";

    private static CsvExportService BuildService(List<JournalEntry> entries)
    {
        var data = new Mock<IDataService>();
        data.Setup(d => d.GetEntriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);
        var logger = new Mock<ILogger<CsvExportService>>();
        return new CsvExportService(data.Object, logger.Object);
    }

    private static async Task<string> ExportAsync(List<JournalEntry> entries)
    {
        var svc = BuildService(entries);
        using var ms = new MemoryStream();
        await svc.ExportToCsvAsync(ms);
        ms.Position = 0;
        using var sr = new StreamReader(ms);
        return await sr.ReadToEndAsync();
    }

    [Fact]
    public async Task Export_EmptyList_WritesHeaderOnly()
    {
        var csv = await ExportAsync(new List<JournalEntry>());

        csv.Should().Be(ExpectedHeader + "\r\n");
    }

    [Fact]
    public async Task Export_SingleEntry_WritesHeaderAndRow()
    {
        var entry = new JournalEntry
        {
            Id = 7,
            CreatedAt = new DateTime(2026, 1, 18, 21, 0, 0, DateTimeKind.Utc),
            Mood = 6,
            SocialAnxiety = 3,
            Regretability = 4,
            Text = "Good night"
        };

        var csv = await ExportAsync(new List<JournalEntry> { entry });
        var lines = csv.Split(new[] { "\r\n" }, StringSplitOptions.None);

        lines.Should().HaveCount(3);
        lines[0].Should().Be(ExpectedHeader);
        lines[1].Should().StartWith("7,");
        lines[1].Should().Contain(",6,3,4,");
        lines[1].Should().EndWith("Good night");
        lines[2].Should().BeEmpty();
    }

    [Fact]
    public async Task Export_TextWithComma_IsQuoted()
    {
        var csv = await ExportAsync(new List<JournalEntry>
        {
            new() { Id = 1, CreatedAt = DateTime.UtcNow, Text = "hello, world" }
        });

        csv.Should().Contain("\"hello, world\"");
    }

    [Fact]
    public async Task Export_TextWithDoubleQuote_IsEscaped()
    {
        var csv = await ExportAsync(new List<JournalEntry>
        {
            new() { Id = 1, CreatedAt = DateTime.UtcNow, Text = "she said \"hi\"" }
        });

        // field wraps: "she said ""hi"""
        csv.Should().Contain("\"she said \"\"hi\"\"\"");
    }

    [Fact]
    public async Task Export_TextWithNewline_IsQuotedAndPreserved()
    {
        var csv = await ExportAsync(new List<JournalEntry>
        {
            new() { Id = 1, CreatedAt = DateTime.UtcNow, Text = "line1\nline2" }
        });

        csv.Should().Contain("\"line1\nline2\"");
    }

    [Fact]
    public void EscapeField_NoSpecialChars_ReturnsRaw()
    {
        CsvExportService.EscapeField("plain text").Should().Be("plain text");
    }

    [Fact]
    public void EscapeField_Null_ReturnsEmpty()
    {
        CsvExportService.EscapeField(null!).Should().BeEmpty();
    }

    [Fact]
    public async Task Export_StreamNotWritable_Throws()
    {
        var svc = BuildService(new List<JournalEntry>());
        using var ms = new MemoryStream();
        ms.Close(); // make it non-writable

        var act = async () => await svc.ExportToCsvAsync(ms);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
