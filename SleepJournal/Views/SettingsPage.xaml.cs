using SleepJournal.ViewModels;

namespace SleepJournal.Views;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsPageViewModel _viewModel;

    public SettingsPage(SettingsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSettingsCommand.ExecuteAsync(default);
    }

    private void OnAudioVolumeChanged(object? sender, EventArgs e)
    {
        if (sender is Slider slider)
        {
            _viewModel.AudioVolume = (float)slider.Value;
        }
    }
}
