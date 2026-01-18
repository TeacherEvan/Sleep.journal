using SleepJournal.Models;
using SleepJournal.Services;
using SleepJournal.ViewModels;
using Microsoft.Extensions.Logging;

namespace SleepJournal.Tests.ViewModels;

/// <summary>
/// Unit tests for MainPageViewModel validating business logic, validation, and command behavior
/// </summary>
public class MainPageViewModelTests
{
    private readonly Mock<IDataService> _mockDataService;
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly Mock<ILogger<MainPageViewModel>> _mockLogger;
    private readonly MainPageViewModel _viewModel;

    public MainPageViewModelTests()
    {
        _mockDataService = new Mock<IDataService>();
        _mockAudioService = new Mock<IAudioService>();
        _mockLogger = new Mock<ILogger<MainPageViewModel>>();
        _viewModel = new MainPageViewModel(_mockDataService.Object, _mockAudioService.Object, _mockLogger.Object);
    }

    #region Property Tests

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _viewModel.Text.Should().BeEmpty();
        _viewModel.Mood.Should().Be(5);
        _viewModel.SocialAnxiety.Should().Be(5);
        _viewModel.Regretability.Should().Be(5);
        _viewModel.ErrorMessage.Should().BeEmpty();
        _viewModel.IsSaving.Should().BeFalse();
    }

    [Theory]
    [InlineData("Valid text")]
    [InlineData("A")]
    [InlineData("This is a longer text with exactly fifty characters!!")]
    public void Text_WhenSet_ShouldUpdateProperty(string text)
    {
        // Act
        _viewModel.Text = text;

        // Assert
        _viewModel.Text.Should().Be(text);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Mood_WhenSetToValidValue_ShouldUpdateProperty(int mood)
    {
        // Act
        _viewModel.Mood = mood;

        // Assert
        _viewModel.Mood.Should().Be(mood);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void SocialAnxiety_WhenSetToValidValue_ShouldUpdateProperty(int anxiety)
    {
        // Act
        _viewModel.SocialAnxiety = anxiety;

        // Assert
        _viewModel.SocialAnxiety.Should().Be(anxiety);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Regretability_WhenSetToValidValue_ShouldUpdateProperty(int regretability)
    {
        // Act
        _viewModel.Regretability = regretability;

        // Assert
        _viewModel.Regretability.Should().Be(regretability);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task SaveCommand_WithEmptyText_ShouldSetErrorMessage()
    {
        // Arrange
        _viewModel.Text = "";

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Please enter some text for your journal entry.");
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveCommand_WithWhitespaceText_ShouldSetErrorMessage()
    {
        // Arrange
        _viewModel.Text = "   ";

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Please enter some text for your journal entry.");
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveCommand_WithTextOver200Characters_ShouldSetErrorMessage()
    {
        // Arrange
        _viewModel.Text = new string('A', 201);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Text must be 200 characters or less.");
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public async Task SaveCommand_WithInvalidMood_ShouldSetErrorMessage(int mood)
    {
        // Arrange
        _viewModel.Text = "Valid text";
        _viewModel.Mood = mood;

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Mood must be between 1 and 10.");
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-5)]
    public async Task SaveCommand_WithInvalidSocialAnxiety_ShouldSetErrorMessage(int anxiety)
    {
        // Arrange
        _viewModel.Text = "Valid text";
        _viewModel.SocialAnxiety = anxiety;

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Social Anxiety must be between 1 and 10.");
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-3)]
    public async Task SaveCommand_WithInvalidRegretability_ShouldSetErrorMessage(int regretability)
    {
        // Arrange
        _viewModel.Text = "Valid text";
        _viewModel.Regretability = regretability;

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Regretability must be between 1 and 10.");
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Save Command Tests

    [Fact]
    public async Task SaveCommand_WithValidData_ShouldCallDataService()
    {
        // Arrange
        _viewModel.Text = "Today was a good day";
        _viewModel.Mood = 7;
        _viewModel.SocialAnxiety = 3;
        _viewModel.Regretability = 2;

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockDataService.Verify(s => s.SaveEntryAsync(
            It.Is<JournalEntry>(e =>
                e.Text == "Today was a good day" &&
                e.Mood == 7 &&
                e.SocialAnxiety == 3 &&
                e.Regretability == 2),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveCommand_WithValidData_ShouldTrimText()
    {
        // Arrange
        _viewModel.Text = "  Text with spaces  ";

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockDataService.Verify(s => s.SaveEntryAsync(
            It.Is<JournalEntry>(e => e.Text == "Text with spaces"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveCommand_WhenSuccessful_ShouldResetForm()
    {
        // Arrange
        _viewModel.Text = "Journal entry";
        _viewModel.Mood = 8;
        _viewModel.SocialAnxiety = 4;
        _viewModel.Regretability = 3;

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Text.Should().BeEmpty();
        _viewModel.Mood.Should().Be(5);
        _viewModel.SocialAnxiety.Should().Be(5);
        _viewModel.Regretability.Should().Be(5);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveCommand_WhenSuccessful_ShouldClearErrorMessage()
    {
        // Arrange
        _viewModel.Text = "Valid entry";
        _viewModel.ErrorMessage = "Previous error";

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveCommand_WhenFails_ShouldSetErrorMessage()
    {
        // Arrange
        _viewModel.Text = "Journal entry";

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Failed to save entry. Please try again.");
    }

    [Fact]
    public async Task SaveCommand_WhenCancelled_ShouldSetCancellationMessage()
    {
        // Arrange
        _viewModel.Text = "Journal entry";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(cts.Token);

        // Assert
        _viewModel.ErrorMessage.Should().Be("Save operation was cancelled.");
    }

    [Fact]
    public async Task SaveCommand_ShouldSetIsSavingToTrueDuringOperation()
    {
        // Arrange
        _viewModel.Text = "Journal entry";
        var taskCompletionSource = new TaskCompletionSource();

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(taskCompletionSource.Task);

        // Act
        var saveTask = _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert - should be saving
        _viewModel.IsSaving.Should().BeTrue();

        // Complete the save
        taskCompletionSource.SetResult();
        await saveTask;

        // Assert - should no longer be saving
        _viewModel.IsSaving.Should().BeFalse();
    }

    [Fact]
    public void SaveCommand_CanExecute_ShouldBeFalseWhenTextIsEmpty()
    {
        // Arrange
        _viewModel.Text = "";

        // Assert
        _viewModel.SaveCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void SaveCommand_CanExecute_ShouldBeFalseWhenTextIsTooLong()
    {
        // Arrange
        _viewModel.Text = new string('A', 201);

        // Assert
        _viewModel.SaveCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void SaveCommand_CanExecute_ShouldBeTrueWithValidText()
    {
        // Arrange
        _viewModel.Text = "Valid entry";

        // Assert
        _viewModel.SaveCommand.CanExecute(null).Should().BeTrue();
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public async Task SaveCommand_WithExactly200Characters_ShouldSucceed()
    {
        // Arrange
        _viewModel.Text = new string('A', 200);

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveCommand_WithSingleCharacter_ShouldSucceed()
    {
        // Arrange
        _viewModel.Text = "A";

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    public async Task SaveCommand_WithBoundaryMoodValues_ShouldSucceed(int mood)
    {
        // Arrange
        _viewModel.Text = "Valid text";
        _viewModel.Mood = mood;

        _mockDataService.Setup(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockDataService.Verify(s => s.SaveEntryAsync(It.IsAny<JournalEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
