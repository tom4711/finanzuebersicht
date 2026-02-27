using Finanzuebersicht.Services;

namespace Finanzuebersicht;

public partial class App : Application
{
	public App(InitializationService initService)
	{
		InitializeComponent();
		Task.Run(async () => await initService.InitializeAsync());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}