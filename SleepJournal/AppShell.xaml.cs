using SleepJournal.Views;

namespace SleepJournal;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register welcome page route
		Routing.RegisterRoute("WelcomePage", typeof(WelcomePage));
	}
}
