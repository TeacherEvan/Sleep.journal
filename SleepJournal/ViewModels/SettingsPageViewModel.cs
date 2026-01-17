using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepJournal.Models;
using SleepJournal.Services;
using Microsoft.Extensions.Logging;

namespace SleepJournal.ViewModels;

/// <summary>
/// ViewModel for the settings page managing user preferences.
/// </summary>
public partial class SettingsPageViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ILogger<SettingsPageViewModel> _logger;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private bool enableReminders;

    [ObservableProperty]
    private TimeSpan reminderTime = new(21, 0, 0); // Default 9:00 PM

    [ObservableProperty]
    private bool useDarkMode;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string successMessage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPageViewModel"/> class.
    /// </summary>
    /// <param name="dataService">Data service for database operations.</param>
    /// <param name="logger">Logger instance for tracking operations.</param>
    public SettingsPageViewModel(IDataService dataService, ILogger<SettingsPageViewModel> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    /// <summary>
    /// Loads user settings when navigating to the page.
    /// </summary>
    [RelayCommand]
    private async Task LoadSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var settings = await _dataService.GetUserSettingsAsync(cancellationToken);

            if (settings != null)
            {
                UserName = settings.UserName ?? string.Empty;
                EnableReminders = settings.EnableReminders;
                ReminderTime = settings.ReminderTime;
                UseDarkMode = settings.UseDarkMode;

                _logger.LogInformation("Loaded user settings for user: {UserName}", UserName);
            }
            else
            {
                _logger.LogInformation("No existing settings found, using defaults");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user settings");
            ErrorMessage = "Failed to load settings. Please try again.";
        }
    }

    /// <summary>
    /// Saves user settings to the database.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var trimmedUserName = UserName.Trim();
            if (string.IsNullOrWhiteSpace(trimmedUserName))
            {
                ErrorMessage = "Please enter a user name.";
                return;
            }

            var settings = new UserSettings
            {
                UserName = trimmedUserName,
                EnableReminders = EnableReminders,
                ReminderTime = ReminderTime,
                UseDarkMode = UseDarkMode
            };

            await _dataService.SaveUserSettingsAsync(settings, cancellationToken);

            SuccessMessage = "Settings saved successfully!";
            _logger.LogInformation("Saved user settings for: {UserName}", trimmedUserName);

            // Apply dark mode immediately
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = UseDarkMode ? AppTheme.Dark : AppTheme.Light;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user settings");
            ErrorMessage = "Failed to save settings. Please try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanSave => !IsSaving && !string.IsNullOrWhiteSpace(UserName);

    partial void OnUserNameChanged(string value)
    {
        SaveSettingsCommand.NotifyCanExecuteChanged();
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    partial void OnIsSavingChanged(bool value)
    {
        SaveSettingsCommand.NotifyCanExecuteChanged();
    }
}
