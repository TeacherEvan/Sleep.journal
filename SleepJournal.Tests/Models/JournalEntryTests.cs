using SleepJournal.Models;

namespace SleepJournal.Tests.Models;

/// <summary>
/// Unit tests for JournalEntry model validating data integrity and constraints
/// </summary>
public class JournalEntryTests
{
    [Fact]
    public void JournalEntry_DefaultConstructor_ShouldCreateInstanceWithDefaultValues()
    {
        // Act
        var entry = new JournalEntry();

        // Assert
        entry.Should().NotBeNull();
        entry.Id.Should().Be(0);
        entry.Text.Should().BeEmpty();
        entry.Mood.Should().Be(0);
        entry.SocialAnxiety.Should().Be(0);
        entry.Regretability.Should().Be(0);
    }

    [Fact]
    public void JournalEntry_SetProperties_ShouldUpdateValues()
    {
        // Arrange
        var entry = new JournalEntry();
        var now = DateTime.Now;

        // Act
        entry.Id = 1;
        entry.CreatedAt = now;
        entry.Text = "Test entry";
        entry.Mood = 7;
        entry.SocialAnxiety = 4;
        entry.Regretability = 3;

        // Assert
        entry.Id.Should().Be(1);
        entry.CreatedAt.Should().Be(now);
        entry.Text.Should().Be("Test entry");
        entry.Mood.Should().Be(7);
        entry.SocialAnxiety.Should().Be(4);
        entry.Regretability.Should().Be(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short")]
    [InlineData("This is a longer text that represents a typical journal entry with more detail")]
    public void JournalEntry_Text_ShouldAcceptVariousLengths(string text)
    {
        // Arrange
        var entry = new JournalEntry();

        // Act
        entry.Text = text;

        // Assert
        entry.Text.Should().Be(text);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void JournalEntry_Mood_ShouldAcceptValidRange(int mood)
    {
        // Arrange
        var entry = new JournalEntry();

        // Act
        entry.Mood = mood;

        // Assert
        entry.Mood.Should().Be(mood);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void JournalEntry_SocialAnxiety_ShouldAcceptValidRange(int anxiety)
    {
        // Arrange
        var entry = new JournalEntry();

        // Act
        entry.SocialAnxiety = anxiety;

        // Assert
        entry.SocialAnxiety.Should().Be(anxiety);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void JournalEntry_Regretability_ShouldAcceptValidRange(int regretability)
    {
        // Arrange
        var entry = new JournalEntry();

        // Act
        entry.Regretability = regretability;

        // Assert
        entry.Regretability.Should().Be(regretability);
    }

    [Fact]
    public void JournalEntry_CreatedAt_ShouldPreserveDateTimePrecision()
    {
        // Arrange
        var entry = new JournalEntry();
        var specificDate = new DateTime(2026, 1, 17, 14, 30, 45, 123);

        // Act
        entry.CreatedAt = specificDate;

        // Assert
        entry.CreatedAt.Should().Be(specificDate);
    }
}
