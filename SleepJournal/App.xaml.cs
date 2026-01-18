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
		var shell = new AppShell();

		// Check if this is the first run
		var isFirstRun = Preferences.Get(FirstRunKey, true);

		if (isFirstRun)
		{
			// Show welcome screen on first run
			shell.CurrentItem = shell.Items[0]; // Default to first item
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				await Shell.Current.GoToAsync("//WelcomePage");
				Preferences.Set(FirstRunKey, false);
			});
		}

		return new Window(shell);
	}
}