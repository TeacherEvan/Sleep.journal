using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SleepJournal.Services;

namespace SleepJournal.ViewModels;

/// <summary>
/// ViewModel for the welcome/onboarding screen.
/// </summary>
public partial class WelcomePageViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<WelcomePageViewModel> _logger;

    [ObservableProperty]
    private bool isRequestingPermissions;

    public WelcomePageViewModel(
        INotificationService notificationService,
        ILogger<WelcomePageViewModel> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Requests notification permissions when the page appears.
    /// </summary>
    [RelayCommand]
    private async Task PageAppearingAsync()
    {
        try
        {
            IsRequestingPermissions = true;
            _logger.LogInformation("Requesting notification permissions on welcome screen");

            // Request notification permissions proactively
            var granted = await _notificationService.RequestPermissionsAsync();

            if (granted)
            {
                _logger.LogInformation("Notification permissions granted");
            }
            else
            {
                _logger.LogWarning("Notification permissions denied by user");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to request notification permissions");
        }
        finally
        {
            IsRequestingPermissions = false;
        }
    }

    /// <summary>
    /// Navigates to the main app after welcome screen.
    /// </summary>
    [RelayCommand]
    private async Task GetStartedAsync()
    {
        try
        {
            _logger.LogInformation("User completed welcome screen, navigating to main app");

            // Navigate to main page
            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate from welcome screen");
        }
    }
}
