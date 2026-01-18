namespace SleepJournal;

public partial class App : Application
{
	private const string FirstRunKey = "IsFirstRun";

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Get service provider from the activation state's context
		var serviceProvider = activationState?.Context?.Services
			?? IPlatformApplication.Current?.Services
			?? throw new InvalidOperationException("Cannot get service provider");

		var shell = serviceProvider.GetRequiredService<AppShell>();

		// Check if this is the first run
		var isFirstRun = Preferences.Get(FirstRunKey, true);

		if (isFirstRun)
		{
			// Show welcome screen on first run
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Shell.Current.GoToAsync(nameof(Views.WelcomePage));
				Preferences.Set(FirstRunKey, false);
			});
		}

		return new Window(shell);
	}
}