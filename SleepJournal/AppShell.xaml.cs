using SleepJournal.Views;

namespace SleepJournal;

public partial class AppShell : Shell
{
	public AppShell(IServiceProvider serviceProvider)
	{
		InitializeComponent();

		// Register routes for navigation - Shell will resolve from DI
		Routing.RegisterRoute(nameof(WelcomePage), typeof(WelcomePage));
		Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
		Routing.RegisterRoute(nameof(HistoryPage), typeof(HistoryPage));
		Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
		Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

		// Build flyout menu using ShellContent with DI-resolved pages
		var journalItem = new FlyoutItem
		{
			Title = "Journal",
			Icon = "home.png",
			Route = nameof(MainPage)
		};
		journalItem.Items.Add(new ShellContent
		{
			Title = "New Entry",
			Route = nameof(MainPage),
			Content = serviceProvider.GetRequiredService<MainPage>()
		});

		var statsItem = new FlyoutItem
		{
			Title = "Statistics",
			Icon = "chart.png",
			Route = nameof(StatisticsPage)
		};
		statsItem.Items.Add(new ShellContent
		{
			Title = "Analytics",
			Route = nameof(StatisticsPage),
			Content = serviceProvider.GetRequiredService<StatisticsPage>()
		});

		var historyItem = new FlyoutItem
		{
			Title = "History",
			Icon = "list.png",
			Route = nameof(HistoryPage)
		};
		historyItem.Items.Add(new ShellContent
		{
			Title = "View Entries",
			Route = nameof(HistoryPage),
			Content = serviceProvider.GetRequiredService<HistoryPage>()
		});

		var settingsItem = new FlyoutItem
		{
			Title = "Settings",
			Icon = "settings.png",
			Route = nameof(SettingsPage)
		};
		settingsItem.Items.Add(new ShellContent
		{
			Title = "Preferences",
			Route = nameof(SettingsPage),
			Content = serviceProvider.GetRequiredService<SettingsPage>()
		});

		Items.Add(journalItem);
		Items.Add(statsItem);
		Items.Add(historyItem);
		Items.Add(settingsItem);
	}
}
