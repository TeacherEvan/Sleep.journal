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
    private readonly IBiometricService _biometricService;
    private readonly ILogger<SettingsPageViewModel> _logger;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private bool enableReminders;

    [ObservableProperty]
    private TimeSpan reminderTime = AppConstants.Defaults.ReminderTime; // Default 9:00 PM

    [ObservableProperty]
    private bool useDarkMode;

    [ObservableProperty]
    private bool biometricAuthEnabled;

    [ObservableProperty]
    private bool biometricAvailable;

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
    /// <param name="biometricService">Biometric authentication service.</param>
    /// <param name="logger">Logger instance for tracking operations.</param>
    public SettingsPageViewModel(
        IDataService dataService, 
        IBiometricService biometricService,
        ILogger<SettingsPageViewModel> logger)
    {
        _dataService = dataService;
        _biometricService = biometricService;
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

            // Load biometric availability
            BiometricAvailable = await _biometricService.IsAvailableAsync();
            BiometricAuthEnabled = await _biometricService.IsBiometricEnabledAsync();

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
    /// Toggles biometric authentication and records the preference.
    /// </summary>
    partial void OnBiometricAuthEnabledChanged(bool value)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await _biometricService.SetBiometricEnabledAsync(value);
                _logger.LogInformation("Biometric authentication {Status}", value ? "enabled" : "disabled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle biometric authentication");
                ErrorMessage = "Failed to update biometric settings.";
            }
        });
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
