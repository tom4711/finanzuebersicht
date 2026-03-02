using Finanzuebersicht.Services;

namespace Finanzuebersicht;

public partial class App : Application
{
	private readonly IDataService _dataService;
	private readonly InitializationService _initService;
	private readonly ThemeService _themeService;
	private readonly string _savedTheme;

	public App(InitializationService initService, IDataService dataService, SettingsService settings, ThemeService themeService)
	{
		InitializeComponent();
		_dataService = dataService;
		_initService = initService;
		_themeService = themeService;

		// Gespeichertes Theme anwenden (MAUI-Ebene)
		_savedTheme = settings.Get("Theme", "System");
		_themeService.Apply(_savedTheme);
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

		// UIKit-Style nach Window-Erstellung setzen
		window.Created += (_, _) => _themeService.Apply(_savedTheme);

		window.Resumed += async (s, e) =>
		{
			await _dataService.GeneratePendingRecurringTransactionsAsync();
		};

		return window;
	}

	protected override async void OnStart()
	{
		base.OnStart();
		await _initService.InitializeAsync();
		await _dataService.GeneratePendingRecurringTransactionsAsync();
	}
}