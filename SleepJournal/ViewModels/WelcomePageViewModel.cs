using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace SleepJournal.ViewModels;

/// <summary>
/// ViewModel for the welcome/onboarding screen.
/// </summary>
public partial class WelcomePageViewModel : ObservableObject
{
    private readonly ILogger<WelcomePageViewModel> _logger;

    public WelcomePageViewModel(ILogger<WelcomePageViewModel> logger)
    {
        _logger = logger;
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
