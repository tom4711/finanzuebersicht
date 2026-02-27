using Finanzuebersicht.Services;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht;

public partial class App : Application
{
	private readonly IDataService _dataService;
	private readonly string _savedTheme;

	public App(InitializationService initService, IDataService dataService, SettingsService settings)
	{
		InitializeComponent();
		_dataService = dataService;

		// Gespeichertes Theme anwenden (MAUI-Ebene)
		_savedTheme = settings.Get("Theme", "System");
		SettingsViewModel.ApplyTheme(_savedTheme);

		Task.Run(async () => await initService.InitializeAsync());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

		// UIKit-Style nach Window-Erstellung setzen
		window.Created += (_, _) => SettingsViewModel.ApplyTheme(_savedTheme);

		window.Resumed += async (s, e) =>
		{
			await _dataService.GeneratePendingRecurringTransactionsAsync();
		};

		return window;
	}

	protected override async void OnStart()
	{
		base.OnStart();
		await _dataService.GeneratePendingRecurringTransactionsAsync();
	}
}