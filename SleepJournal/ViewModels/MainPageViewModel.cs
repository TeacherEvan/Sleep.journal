using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepJournal.Models;
using SleepJournal.Services;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace SleepJournal.ViewModels;

/// <summary>
/// ViewModel for the main page, handling journal entry creation and validation.
/// Implements MVVM pattern using CommunityToolkit.Mvvm source generators.
/// Supports both creating new entries and editing existing ones via query parameters.
/// </summary>
public partial class MainPageViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;
    private readonly IAudioService _audioService;
    private readonly ILogger<MainPageViewModel> _logger;
    private int? _editingEntryId;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainPageViewModel"/> class.
    /// </summary>
    /// <param name="dataService">Data service for persisting journal entries.</param>
    /// <param name="audioService">Audio service for playing feedback sounds.</param>
    /// <param name="logger">Logger instance for tracking operations.</param>
    public MainPageViewModel(IDataService dataService, IAudioService audioService, ILogger<MainPageViewModel> logger)
    {
        _dataService = dataService;
        _audioService = audioService;
        _logger = logger;
    }

    /// <summary>
    /// Gets or sets the page title (changes based on edit mode).
    /// </summary>
    [ObservableProperty]
    private string pageTitle = "New Journal Entry";

    /// <summary>
    /// Gets or sets the save button text (changes based on edit mode).
    /// </summary>
    [ObservableProperty]
    private string saveButtonText = "Save Entry";

    /// <summary>
    /// Gets or sets the journal entry text (max 200 characters).
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string text = "";

    /// <summary>
    /// Gets or sets the mood rating (1-10 scale).
    /// </summary>
    [ObservableProperty]
    private int mood = AppConstants.Validation.DefaultRating;

    /// <summary>
    /// Gets or sets the social anxiety level (1-10 scale).
    /// </summary>
    [ObservableProperty]
    private int socialAnxiety = AppConstants.Validation.DefaultRating;

    /// <summary>
    /// Gets or sets the regretability rating (1-10 scale).
    /// </summary>
    [ObservableProperty]
    private int regretability = AppConstants.Validation.DefaultRating;

    /// <summary>
    /// Gets or sets the error message displayed to the user.
    /// </summary>
    [ObservableProperty]
    private string errorMessage = "";

    /// <summary>
    /// Gets or sets a value indicating whether a save operation is in progress.
    /// </summary>
    [ObservableProperty]
    private bool isSaving;

    /// <summary>
    /// Determines whether the save command can execute based on validation rules.
    /// </summary>
    private bool CanSave => !string.IsNullOrWhiteSpace(Text) &&
                            Text.Length <= AppConstants.Validation.MaxTextLength &&
                            !IsSaving;

    /// <summary>
    /// Saves the current journal entry to the database.
    /// Validates input, creates or updates entry, and resets the form on success.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (!ValidateInput())
            return;

        IsSaving = true;
        ErrorMessage = "";

        // Play drop sound on button click
        _ = _audioService.PlayDropSoundAsync(cancellationToken);

        try
        {
            var entry = new JournalEntry
            {
                Id = _editingEntryId ?? 0,
                CreatedAt = DateTime.Now,
                Text = Text.Trim(),
                Mood = Mood,
                SocialAnxiety = SocialAnxiety,
                Regretability = Regretability
            };

            await _dataService.SaveEntryAsync(entry, cancellationToken);

            var action = _editingEntryId.HasValue ? "updated" : "saved";
            _logger.LogInformation("Journal entry {Action} successfully", action);

            // Reset form for new entries, navigate back for edits
            if (_editingEntryId.HasValue)
            {
                // Navigate back to history after editing (only when Shell is available)
                if (Shell.Current != null)
                {
                    try
                    {
                        await Shell.Current.GoToAsync("..");
                    }
                    catch (Exception navEx)
                    {
                        _logger.LogWarning(navEx, "Failed to navigate back after saving");
                    }
                }
            }
            else
            {
                // Reset form for new entries
                ResetForm();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Save operation was cancelled");
            ErrorMessage = "Save operation was cancelled.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save journal entry");
            ErrorMessage = "Failed to save entry. Please try again.";
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
            ErrorMessage = "Please enter some text for your journal entry.";
            return false;
        }

        if (Text.Length > AppConstants.Validation.MaxTextLength)
        {
            ErrorMessage = $"Text must be {AppConstants.Validation.MaxTextLength} characters or less.";
            return false;
        }

        if (Mood < AppConstants.Validation.MinRating || Mood > AppConstants.Validation.MaxRating)
        {
            ErrorMessage = $"Mood must be between {AppConstants.Validation.MinRating} and {AppConstants.Validation.MaxRating}.";
            return false;
        }

        if (SocialAnxiety < AppConstants.Validation.MinRating || SocialAnxiety > AppConstants.Validation.MaxRating)
        {
            ErrorMessage = $"Social Anxiety must be between {AppConstants.Validation.MinRating} and {AppConstants.Validation.MaxRating}.";
            return false;
        }

        if (Regretability < AppConstants.Validation.MinRating || Regretability > AppConstants.Validation.MaxRating)
        {
            ErrorMessage = $"Regretability must be between {AppConstants.Validation.MinRating} and {AppConstants.Validation.MaxRating}.";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Receives query parameters when navigating to this page (for edit mode).
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("entryId") && int.TryParse(query["entryId"].ToString(), out var entryId))
        {
            _editingEntryId = entryId;
            PageTitle = "Edit Journal Entry";
            SaveButtonText = "Update Entry";

            // Load the entry asynchronously
            _ = LoadEntryForEditAsync(entryId);
        }
        else
        {
            // New entry mode
            _editingEntryId = null;
            PageTitle = "New Journal Entry";
            SaveButtonText = "Save Entry";
            ResetForm();
        }
    }

    /// <summary>
    /// Loads an existing entry for editing.
    /// </summary>
    private async Task LoadEntryForEditAsync(int entryId)
    {
        try
        {
            var entry = await _dataService.GetJournalEntryByIdAsync(entryId);

            if (entry != null)
            {
                Text = entry.Text;
                Mood = entry.Mood;
                SocialAnxiety = entry.SocialAnxiety;
                Regretability = entry.Regretability;

                _logger.LogInformation("Loaded entry {EntryId} for editing", entryId);
            }
            else
            {
                ErrorMessage = "Entry not found.";
                _logger.LogWarning("Entry {EntryId} not found", entryId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load entry {EntryId}", entryId);
            ErrorMessage = "Failed to load entry.";
        }
    }

    /// <summary>
    /// Resets all form fields to their default values.
    /// </summary>
    private void ResetForm()
    {
        Text = "";
        Mood = AppConstants.Validation.DefaultRating;
        SocialAnxiety = AppConstants.Validation.DefaultRating;
        Regretability = AppConstants.Validation.DefaultRating;
        ErrorMessage = "";
        _editingEntryId = null;
    }
}