using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepJournal.Models;
using SleepJournal.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace SleepJournal.ViewModels;

/// <summary>
/// ViewModel for the journal entry history page with pagination support.
/// </summary>
public partial class HistoryPageViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ILogger<HistoryPageViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<JournalEntry> entries = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool hasMoreItems = true;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SearchCommand))]
    private string searchText = string.Empty;

    [ObservableProperty]
    private int? minMood;

    [ObservableProperty]
    private int? maxMood;

    [ObservableProperty]
    private DateTime? startDate;

    [ObservableProperty]
    private DateTime? endDate;

    [ObservableProperty]
    private bool isFiltered;

    private int _currentPage;
    private List<JournalEntry> _allEntries = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryPageViewModel"/> class.
    /// </summary>
    /// <param name="dataService">Data service for database operations.</param>
    /// <param name="logger">Logger instance for tracking operations.</param>
    public HistoryPageViewModel(IDataService dataService, ILogger<HistoryPageViewModel> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    /// <summary>
    /// Loads the initial page of entries when navigating to the history page.
    /// </summary>
    [RelayCommand]
    private async Task LoadEntriesAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _currentPage = 0;

            _allEntries = (await _dataService.GetJournalEntriesAsync(cancellationToken)).ToList();
            var filteredItems = ApplyFilters(_allEntries);
            var pagedItems = filteredItems.Take(AppConstants.Pagination.PageSize).ToList();

            Entries.Clear();
            foreach (var entry in pagedItems)
            {
                Entries.Add(entry);
            }

            HasMoreItems = filteredItems.Count > AppConstants.Pagination.PageSize;
            _currentPage = 1;

            _logger.LogInformation("Loaded {Count} journal entries (page 1), filtered from {Total}", Entries.Count, _allEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load journal entries");
            ErrorMessage = "Failed to load entries. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads the next page of entries for infinite scrolling.
    /// </summary>
    [RelayCommand]
    private async Task LoadMoreEntriesAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoading || !HasMoreItems)
            return;

        try
        {
            IsLoading = true;

            var filteredItems = ApplyFilters(_allEntries);
            var pagedItems = filteredItems.Skip(_currentPage * AppConstants.Pagination.PageSize).Take(AppConstants.Pagination.PageSize).ToList();

            foreach (var entry in pagedItems)
            {
                Entries.Add(entry);
            }

            HasMoreItems = filteredItems.Count > (_currentPage + 1) * AppConstants.Pagination.PageSize;
            _currentPage++;

            _logger.LogInformation("Loaded page {Page} with {Count} entries", _currentPage, pagedItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load more entries");
            ErrorMessage = "Failed to load more entries.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Refreshes the entry list by reloading from the beginning.
    /// </summary>
    [RelayCommand]
    private async Task RefreshEntriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsRefreshing = true;
            await LoadEntriesAsync(cancellationToken);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Deletes a journal entry after confirmation.
    /// </summary>
    [RelayCommand]
    private async Task DeleteEntryAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        try
        {
            bool confirmed = false;
            if (Application.Current?.MainPage != null)
            {
                confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Delete Entry",
                    $"Are you sure you want to delete the entry from {entry.EntryDate:MMM dd, yyyy}?",
                    "Delete",
                    "Cancel");
            }

            if (!confirmed)
                return;

            await _dataService.DeleteJournalEntryAsync(entry.Id, cancellationToken);
            Entries.Remove(entry);

            _logger.LogInformation("Deleted journal entry {EntryId}", entry.Id);

            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Success",
                    "Entry deleted successfully",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete entry {EntryId}", entry.Id);
            ErrorMessage = "Failed to delete entry. Please try again.";
        }
    }

    /// <summary>
    /// Navigates to edit an existing entry.
    /// </summary>
    [RelayCommand]
    private async Task EditEntryAsync(JournalEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        try
        {
            await Shell.Current.GoToAsync($"MainPage?entryId={entry.Id}");
            _logger.LogInformation("Navigating to edit entry {EntryId}", entry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to edit entry {EntryId}", entry.Id);
            ErrorMessage = "Failed to open entry for editing.";
        }
    }

    /// <summary>
    /// Searches entries based on search text.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task SearchAsync(CancellationToken cancellationToken = default)
    {
        await LoadEntriesAsync(cancellationToken);
        IsFiltered = !string.IsNullOrWhiteSpace(SearchText) || MinMood.HasValue || MaxMood.HasValue || StartDate.HasValue || EndDate.HasValue;
        _logger.LogInformation("Search executed with text: '{SearchText}'", SearchText);
    }

    private bool CanSearch => !IsLoading;

    /// <summary>
    /// Clears all filters and reloads all entries.
    /// </summary>
    [RelayCommand]
    private async Task ClearFiltersAsync(CancellationToken cancellationToken = default)
    {
        SearchText = string.Empty;
        MinMood = null;
        MaxMood = null;
        StartDate = null;
        EndDate = null;
        IsFiltered = false;
        await LoadEntriesAsync(cancellationToken);
        _logger.LogInformation("Filters cleared");
    }

    /// <summary>
    /// Applies search and filter criteria to the entry list.
    /// </summary>
    private List<JournalEntry> ApplyFilters(List<JournalEntry> entries)
    {
        var filtered = entries.AsEnumerable();

        // Text search
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(e => e.Text.ToLowerInvariant().Contains(searchLower));
        }

        // Mood filters
        if (MinMood.HasValue)
        {
            filtered = filtered.Where(e => e.Mood >= MinMood.Value);
        }

        if (MaxMood.HasValue)
        {
            filtered = filtered.Where(e => e.Mood <= MaxMood.Value);
        }

        // Date range filters
        if (StartDate.HasValue)
        {
            filtered = filtered.Where(e => e.CreatedAt.Date >= StartDate.Value.Date);
        }

        if (EndDate.HasValue)
        {
            filtered = filtered.Where(e => e.CreatedAt.Date <= EndDate.Value.Date);
        }

        return filtered.ToList();
    }
}
