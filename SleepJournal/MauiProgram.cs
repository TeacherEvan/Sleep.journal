using Microsoft.Extensions.Logging;
using SleepJournal.Services;
using SleepJournal.ViewModels;
using SleepJournal.Views;

namespace SleepJournal;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<IDataService, SQLiteDataService>();
		builder.Services.AddSingleton<IBiometricService, BiometricService>();

		// ViewModels
		builder.Services.AddTransient<MainPageViewModel>();
		builder.Services.AddTransient<HistoryPageViewModel>();
		builder.Services.AddTransient<SettingsPageViewModel>();
		builder.Services.AddTransient<StatisticsViewModel>();

		// Pages
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<HistoryPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<StatisticsPage>();

#if DEBUG
		builder.Logging.AddDebug();
		builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
		builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

		return builder.Build();
	}
}
