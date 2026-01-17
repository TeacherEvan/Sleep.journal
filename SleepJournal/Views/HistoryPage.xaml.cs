using SleepJournal.ViewModels;

namespace SleepJournal.Views;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryPageViewModel _viewModel;

    public HistoryPage(HistoryPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadEntriesCommand.ExecuteAsync(default);
    }
}
