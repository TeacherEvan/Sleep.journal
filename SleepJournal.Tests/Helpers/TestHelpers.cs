using Microsoft.Extensions.Logging;

namespace SleepJournal.Tests.Helpers;

/// <summary>
/// Test helper utilities for common testing scenarios
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a mock logger that can be used in tests
    /// </summary>
    public static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Creates a test journal entry with default values
    /// </summary>
    public static Models.JournalEntry CreateTestEntry(
        string text = "Test entry",
        int mood = 5,
        int socialAnxiety = 5,
        int regretability = 5,
        DateTime? createdAt = null)
    {
        return new Models.JournalEntry
        {
            Text = text,
            Mood = mood,
            SocialAnxiety = socialAnxiety,
            Regretability = regretability,
            CreatedAt = createdAt ?? DateTime.Now
        };
    }

    /// <summary>
    /// Creates multiple test entries with sequential text
    /// </summary>
    public static IEnumerable<Models.JournalEntry> CreateTestEntries(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            yield return CreateTestEntry(
                text: $"Entry {i}",
                mood: (i % 10) + 1,
                socialAnxiety: ((i * 2) % 10) + 1,
                regretability: ((i * 3) % 10) + 1,
                createdAt: DateTime.Now.AddHours(-count + i)
            );
        }
    }

    /// <summary>
    /// Verifies that a logger was called with a specific log level
    /// </summary>
    public static void VerifyLog<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel level,
        Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    /// <summary>
    /// Creates a cancellation token that is already cancelled
    /// </summary>
    public static CancellationToken CreateCancelledToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        return cts.Token;
    }

    /// <summary>
    /// Creates a temporary database path for testing
    /// </summary>
    public static string GetTempDbPath()
    {
        return Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
    }

    /// <summary>
    /// Asserts that two journal entries are equivalent (ignoring Id)
    /// </summary>
    public static void AssertEntriesEquivalent(
        Models.JournalEntry expected,
        Models.JournalEntry actual)
    {
        actual.Text.Should().Be(expected.Text);
        actual.Mood.Should().Be(expected.Mood);
        actual.SocialAnxiety.Should().Be(expected.SocialAnxiety);
        actual.Regretability.Should().Be(expected.Regretability);
        actual.CreatedAt.Should().BeCloseTo(expected.CreatedAt, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Waits for a condition to be true with timeout
    /// </summary>
    public static async Task<bool> WaitForConditionAsync(
        Func<bool> condition,
        TimeSpan timeout)
    {
        var endTime = DateTime.Now + timeout;
        while (DateTime.Now < endTime)
        {
            if (condition())
                return true;

            await Task.Delay(10);
        }

        return false;
    }
}

/// <summary>
/// Test data builder for creating complex test scenarios
/// </summary>
public class JournalEntryBuilder
{
    private string _text = "Default entry";
    private int _mood = 5;
    private int _socialAnxiety = 5;
    private int _regretability = 5;
    private DateTime _createdAt = DateTime.Now;

    public JournalEntryBuilder WithText(string text)
    {
        _text = text;
        return this;
    }

    public JournalEntryBuilder WithMood(int mood)
    {
        _mood = mood;
        return this;
    }

    public JournalEntryBuilder WithSocialAnxiety(int anxiety)
    {
        _socialAnxiety = anxiety;
        return this;
    }

    public JournalEntryBuilder WithRegretability(int regretability)
    {
        _regretability = regretability;
        return this;
    }

    public JournalEntryBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public JournalEntryBuilder CreatedHoursAgo(int hours)
    {
        _createdAt = DateTime.Now.AddHours(-hours);
        return this;
    }

    public Models.JournalEntry Build()
    {
        return new Models.JournalEntry
        {
            Text = _text,
            Mood = _mood,
            SocialAnxiety = _socialAnxiety,
            Regretability = _regretability,
            CreatedAt = _createdAt
        };
    }
}

/// <summary>
/// Custom assertions for Sleep Journal domain
/// </summary>
public static class SleepJournalAssertions
{
    /// <summary>
    /// Asserts that an entry has valid field values
    /// </summary>
    public static void ShouldBeValidEntry(this Models.JournalEntry entry)
    {
        entry.Should().NotBeNull();
        entry.Text.Should().NotBeNullOrWhiteSpace();
        entry.Text.Length.Should().BeLessOrEqualTo(200);
        entry.Mood.Should().BeInRange(1, 10);
        entry.SocialAnxiety.Should().BeInRange(1, 10);
        entry.Regretability.Should().BeInRange(1, 10);
        entry.CreatedAt.Should().NotBe(default(DateTime));
    }

    /// <summary>
    /// Asserts that a collection of entries is in descending chronological order
    /// </summary>
    public static void ShouldBeInDescendingChronologicalOrder(
        this IEnumerable<Models.JournalEntry> entries)
    {
        var entryList = entries.ToList();
        for (int i = 0; i < entryList.Count - 1; i++)
        {
            entryList[i].CreatedAt.Should()
                .BeOnOrAfter(entryList[i + 1].CreatedAt,
                    "entries should be in descending chronological order");
        }
    }
}
