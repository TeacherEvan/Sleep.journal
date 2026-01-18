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
		builder.Services.AddSingleton<IAudioService, AudioService>();
		builder.Services.AddSingleton<INotificationService, NotificationService>();

		// ViewModels
		builder.Services.AddTransient<MainPageViewModel>();
		builder.Services.AddTransient<HistoryPageViewModel>();
		builder.Services.AddTransient<SettingsPageViewModel>();
		builder.Services.AddTransient<StatisticsViewModel>();
		builder.Services.AddTransient<WelcomePageViewModel>();

		// Shell and Pages - Pages used in ShellContent need singleton to avoid recreation
		builder.Services.AddSingleton<AppShell>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<HistoryPage>();
		builder.Services.AddSingleton<SettingsPage>();
		builder.Services.AddSingleton<StatisticsPage>();
		builder.Services.AddTransient<WelcomePage>();

#if DEBUG
		builder.Logging.AddDebug();
		builder.Logging.SetMinimumLevel(LogLevel.Debug);
#else
		builder.Logging.SetMinimumLevel(LogLevel.Information);
#endif

		return builder.Build();
	}
}
