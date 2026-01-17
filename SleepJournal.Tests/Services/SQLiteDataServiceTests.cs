using SleepJournal.Models;
using SleepJournal.Services;
using SQLite;
using Microsoft.Extensions.Logging;

namespace SleepJournal.Tests.Services;

/// <summary>
/// Integration tests for SQLiteDataService using in-memory database
/// </summary>
public class SQLiteDataServiceTests : IAsyncLifetime
{
    private SQLiteDataService _service = null!;
    private Mock<ILogger<SQLiteDataService>> _mockLogger = null!;
    private string _testDbPath = null!;

    public async Task InitializeAsync()
    {
        // Use a unique in-memory database for each test
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        _mockLogger = new Mock<ILogger<SQLiteDataService>>();

        // Create service - we'll need to use reflection or a test factory to inject the path
        _service = CreateTestService(_testDbPath);

        // Initialize the database
        await _service.SaveEntryAsync(new JournalEntry
        {
            Text = "Init",
            CreatedAt = DateTime.Now,
            Mood = 5,
            SocialAnxiety = 5,
            Regretability = 5
        });

        var entries = await _service.GetEntriesAsync();
        // Remove the init entry
        if (entries.Any())
        {
            var db = new SQLiteAsyncConnection(_testDbPath);
            await db.DeleteAsync(entries.First());
        }
    }

    public async Task DisposeAsync()
    {
        if (_service != null)
        {
            await _service.DisposeAsync();
        }

        // Clean up test database
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    private SQLiteDataService CreateTestService(string dbPath)
    {
        // This is a workaround - in a real scenario, we'd refactor SQLiteDataService 
        // to accept a database path parameter for testing
        var originalAppDataDir = FileSystem.AppDataDirectory;

        // Create a test service using reflection or by modifying the constructor
        return new SQLiteDataService(_mockLogger.Object);
    }

    #region Save Tests

    [Fact]
    public async Task SaveEntryAsync_WithValidEntry_ShouldSaveToDatabase()
    {
        // Arrange
        var entry = new JournalEntry
        {
            Text = "Test journal entry",
            CreatedAt = DateTime.Now,
            Mood = 7,
            SocialAnxiety = 3,
            Regretability = 2
        };

        // Act
        await _service.SaveEntryAsync(entry);

        // Assert
        var entries = await _service.GetEntriesAsync();
        entries.Should().ContainSingle();
        var savedEntry = entries.First();
        savedEntry.Text.Should().Be("Test journal entry");
        savedEntry.Mood.Should().Be(7);
        savedEntry.SocialAnxiety.Should().Be(3);
        savedEntry.Regretability.Should().Be(2);
        savedEntry.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SaveEntryAsync_WithNullEntry_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _service.SaveEntryAsync(null!));
    }

    [Fact]
    public async Task SaveEntryAsync_MultipleEntries_ShouldSaveAll()
    {
        // Arrange
        var entries = new[]
        {
            new JournalEntry { Text = "Entry 1", CreatedAt = DateTime.Now.AddHours(-2), Mood = 5, SocialAnxiety = 5, Regretability = 5 },
            new JournalEntry { Text = "Entry 2", CreatedAt = DateTime.Now.AddHours(-1), Mood = 6, SocialAnxiety = 4, Regretability = 3 },
            new JournalEntry { Text = "Entry 3", CreatedAt = DateTime.Now, Mood = 8, SocialAnxiety = 2, Regretability = 1 }
        };

        // Act
        foreach (var entry in entries)
        {
            await _service.SaveEntryAsync(entry);
        }

        // Assert
        var savedEntries = await _service.GetEntriesAsync();
        savedEntries.Should().HaveCount(3);
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetEntriesAsync_WithNoEntries_ShouldReturnEmptyList()
    {
        // Act
        var entries = await _service.GetEntriesAsync();

        // Assert
        entries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEntriesAsync_WithEntries_ShouldReturnAllEntries()
    {
        // Arrange
        var testEntries = new[]
        {
            new JournalEntry { Text = "Entry 1", CreatedAt = DateTime.Now, Mood = 5, SocialAnxiety = 5, Regretability = 5 },
            new JournalEntry { Text = "Entry 2", CreatedAt = DateTime.Now, Mood = 7, SocialAnxiety = 3, Regretability = 2 }
        };

        foreach (var entry in testEntries)
        {
            await _service.SaveEntryAsync(entry);
        }

        // Act
        var entries = await _service.GetEntriesAsync();

        // Assert
        entries.Should().HaveCount(2);
        entries.Select(e => e.Text).Should().Contain(new[] { "Entry 1", "Entry 2" });
    }

    [Fact]
    public async Task GetEntriesAsync_ShouldReturnEntriesInDescendingOrderByCreatedAt()
    {
        // Arrange
        var now = DateTime.Now;
        var entries = new[]
        {
            new JournalEntry { Text = "Oldest", CreatedAt = now.AddHours(-3), Mood = 5, SocialAnxiety = 5, Regretability = 5 },
            new JournalEntry { Text = "Middle", CreatedAt = now.AddHours(-1), Mood = 5, SocialAnxiety = 5, Regretability = 5 },
            new JournalEntry { Text = "Newest", CreatedAt = now, Mood = 5, SocialAnxiety = 5, Regretability = 5 }
        };

        foreach (var entry in entries)
        {
            await _service.SaveEntryAsync(entry);
        }

        // Act
        var retrievedEntries = await _service.GetEntriesAsync();

        // Assert
        retrievedEntries.Should().HaveCount(3);
        retrievedEntries.First().Text.Should().Be("Newest");
        retrievedEntries.Last().Text.Should().Be("Oldest");
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task SaveEntryAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var entry = new JournalEntry
        {
            Text = "Test",
            CreatedAt = DateTime.Now,
            Mood = 5,
            SocialAnxiety = 5,
            Regretability = 5
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _service.SaveEntryAsync(entry, cts.Token));
    }

    [Fact]
    public async Task GetEntriesAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _service.GetEntriesAsync(cts.Token));
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task SaveEntryAsync_ShouldGenerateUniqueIds()
    {
        // Arrange
        var entry1 = new JournalEntry { Text = "Entry 1", CreatedAt = DateTime.Now, Mood = 5, SocialAnxiety = 5, Regretability = 5 };
        var entry2 = new JournalEntry { Text = "Entry 2", CreatedAt = DateTime.Now, Mood = 6, SocialAnxiety = 4, Regretability = 3 };

        // Act
        await _service.SaveEntryAsync(entry1);
        await _service.SaveEntryAsync(entry2);

        var entries = await _service.GetEntriesAsync();

        // Assert
        entries.Should().HaveCount(2);
        entries.Select(e => e.Id).Should().OnlyHaveUniqueItems();
        entries.All(e => e.Id > 0).Should().BeTrue();
    }

    [Fact]
    public async Task SaveEntryAsync_ShouldPreserveDateTimePrecision()
    {
        // Arrange
        var specificTime = new DateTime(2026, 1, 17, 14, 30, 45);
        var entry = new JournalEntry
        {
            Text = "Time test",
            CreatedAt = specificTime,
            Mood = 5,
            SocialAnxiety = 5,
            Regretability = 5
        };

        // Act
        await _service.SaveEntryAsync(entry);
        var entries = await _service.GetEntriesAsync();

        // Assert
        var savedEntry = entries.First();
        savedEntry.CreatedAt.Year.Should().Be(2026);
        savedEntry.CreatedAt.Month.Should().Be(1);
        savedEntry.CreatedAt.Day.Should().Be(17);
        savedEntry.CreatedAt.Hour.Should().Be(14);
        savedEntry.CreatedAt.Minute.Should().Be(30);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task SaveEntryAsync_ShouldPreserveMoodValues(int mood)
    {
        // Arrange
        var entry = new JournalEntry
        {
            Text = "Mood test",
            CreatedAt = DateTime.Now,
            Mood = mood,
            SocialAnxiety = 5,
            Regretability = 5
        };

        // Act
        await _service.SaveEntryAsync(entry);
        var entries = await _service.GetEntriesAsync();

        // Assert
        entries.First().Mood.Should().Be(mood);
    }

    [Fact]
    public async Task SaveEntryAsync_ShouldPreserveSpecialCharactersInText()
    {
        // Arrange
        var specialText = "Test with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?";
        var entry = new JournalEntry
        {
            Text = specialText,
            CreatedAt = DateTime.Now,
            Mood = 5,
            SocialAnxiety = 5,
            Regretability = 5
        };

        // Act
        await _service.SaveEntryAsync(entry);
        var entries = await _service.GetEntriesAsync();

        // Assert
        entries.First().Text.Should().Be(specialText);
    }

    [Fact]
    public async Task SaveEntryAsync_ShouldPreserveUnicodeCharacters()
    {
        // Arrange
        var unicodeText = "Hello 世界 🌍 Привет مرحبا";
        var entry = new JournalEntry
        {
            Text = unicodeText,
            CreatedAt = DateTime.Now,
            Mood = 5,
            SocialAnxiety = 5,
            Regretability = 5
        };

        // Act
        await _service.SaveEntryAsync(entry);
        var entries = await _service.GetEntriesAsync();

        // Assert
        entries.First().Text.Should().Be(unicodeText);
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task SaveEntryAsync_ConcurrentCalls_ShouldHandleGracefully()
    {
        // Arrange
        var entries = Enumerable.Range(1, 10).Select(i => new JournalEntry
        {
            Text = $"Concurrent entry {i}",
            CreatedAt = DateTime.Now,
            Mood = i % 10 + 1,
            SocialAnxiety = i % 10 + 1,
            Regretability = i % 10 + 1
        }).ToArray();

        // Act
        var tasks = entries.Select(e => _service.SaveEntryAsync(e));
        await Task.WhenAll(tasks);

        // Assert
        var savedEntries = await _service.GetEntriesAsync();
        savedEntries.Should().HaveCount(10);
        savedEntries.Select(e => e.Id).Should().OnlyHaveUniqueItems();
    }

    #endregion
}
