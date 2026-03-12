using Finanzuebersicht.Services;

namespace Finanzuebersicht;

public partial class App : global::Microsoft.Maui.Controls.Application
{
	// App-wide event to notify UI of data changes (e.g., after import)
	public static event Action? DataChanged;

		public static void NotifyDataChanged()
		{
			DataChanged?.Invoke();
		}

	private readonly IRecurringGenerationService _recurringGenerationService;
	private readonly InitializationService _initService;
	private readonly ThemeService _themeService;
	private readonly string _savedTheme;

	public App(InitializationService initService, IRecurringGenerationService recurringGenerationService, SettingsService settings,
		ThemeService themeService, ILocalizationService localizationService)
	{
		// Sprache vor InitializeComponent setzen, damit XAML-Bindings korrekt aufgelöst werden
		localizationService.Initialize();

		InitializeComponent();
		_recurringGenerationService = recurringGenerationService;
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
			await _recurringGenerationService.GeneratePendingRecurringTransactionsAsync();
		};

		return window;
	}

	protected override async void OnStart()
	{
		base.OnStart();
		await _initService.InitializeAsync();
		await _recurringGenerationService.GeneratePendingRecurringTransactionsAsync();
	}
}