using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepJournal.Models;
using SleepJournal.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace SleepJournal.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ILogger<MainPageViewModel> _logger;

    public MainPageViewModel(IDataService dataService, ILogger<MainPageViewModel> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string text = "";

    [ObservableProperty]
    [Range(1, 10, ErrorMessage = "Mood must be between 1 and 10")]
    private int mood = 5;

    [ObservableProperty]
    [Range(1, 10, ErrorMessage = "Social Anxiety must be between 1 and 10")]
    private int socialAnxiety = 5;

    [ObservableProperty]
    [Range(1, 10, ErrorMessage = "Regretability must be between 1 and 10")]
    private int regretability = 5;

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool isSaving;

    private bool CanSave => !string.IsNullOrWhiteSpace(Text) &&
                            Text.Length <= 200 &&
                            !IsSaving;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (!ValidateInput())\n            return;

        IsSaving = true;
        ErrorMessage = "";

        try
        {
            var entry = new JournalEntry
            {
                CreatedAt = DateTime.Now,
                Text = Text.Trim(),
                Mood = Mood,
                SocialAnxiety = SocialAnxiety,
                Regretability = Regretability
            };

            await _dataService.SaveEntryAsync(entry, cancellationToken);
            _logger.LogInformation(\"Journal entry saved successfully\");

            // Reset fields
            ResetForm();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(\"Save operation was cancelled\");
            ErrorMessage = \"Save operation was cancelled.\";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, \"Failed to save journal entry\");
            ErrorMessage = \"Failed to save entry. Please try again.\";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            ErrorMessage = \"Please enter some text for your journal entry.\";
            return false;
        }

        if (Text.Length > 200)
        {
            ErrorMessage = \"Text must be 200 characters or less.\";
            return false;
        }

        if (Mood < 1 || Mood > 10)
        {
            ErrorMessage = \"Mood must be between 1 and 10.\";
            return false;
        }

        if (SocialAnxiety < 1 || SocialAnxiety > 10)
        {
            ErrorMessage = \"Social Anxiety must be between 1 and 10.\";
            return false;
        }

        if (Regretability < 1 || Regretability > 10)
        {
            ErrorMessage = \"Regretability must be between 1 and 10.\";
            return false;
        }

        return true;
    }

    private void ResetForm()
    {
        Text = "";
        Mood = 5;
        SocialAnxiety = 5;
        Regretability = 5;
        ErrorMessage = "";
    }
}