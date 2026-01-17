using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SleepJournal.Models;
using SleepJournal.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SleepJournal.ViewModels;

/// <summary>
/// ViewModel for statistics dashboard showing journal entry analytics.
/// </summary>
public partial class StatisticsViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly ILogger<StatisticsViewModel> _logger;

    public StatisticsViewModel(IDataService dataService, ILogger<StatisticsViewModel> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    [ObservableProperty]
    private int totalEntries;

    [ObservableProperty]
    private double averageMood;

    [ObservableProperty]
    private double averageSocialAnxiety;

    [ObservableProperty]
    private double averageRegretability;

    [ObservableProperty]
    private int entriesLast7Days;

    [ObservableProperty]
    private int entriesLast30Days;

    [ObservableProperty]
    private string mostCommonMood = "N/A";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool hasData;

    /// <summary>
    /// Loads statistics data when the page appears.
    /// </summary>
    [RelayCommand]
    private async Task LoadStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = "";
            _logger.LogInformation("Loading statistics...");

            var entries = await _dataService.GetJournalEntriesAsync(cancellationToken);

            if (!entries.Any())
            {
                HasData = false;
                _logger.LogInformation("No entries found for statistics");
                return;
            }

            HasData = true;
            TotalEntries = entries.Count;

            // Calculate averages
            AverageMood = Math.Round(entries.Average(e => e.Mood), 1);
            AverageSocialAnxiety = Math.Round(entries.Average(e => e.SocialAnxiety), 1);
            AverageRegretability = Math.Round(entries.Average(e => e.Regretability), 1);

            // Time-based counts
            var now = DateTime.Now;
            var sevenDaysAgo = now.AddDays(-7);
            var thirtyDaysAgo = now.AddDays(-30);

            EntriesLast7Days = entries.Count(e => e.CreatedAt >= sevenDaysAgo);
            EntriesLast30Days = entries.Count(e => e.CreatedAt >= thirtyDaysAgo);

            // Most common mood (group by rounded mood)
            var moodGroups = entries
                .GroupBy(e => e.Mood)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (moodGroups != null)
            {
                var moodValue = moodGroups.Key;
                var count = moodGroups.Count();
                MostCommonMood = $"{moodValue}/10 ({count} entries)";
            }

            _logger.LogInformation("Statistics loaded successfully. Total entries: {Count}", TotalEntries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics");
            ErrorMessage = "Failed to load statistics. Please try again.";
            HasData = false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
