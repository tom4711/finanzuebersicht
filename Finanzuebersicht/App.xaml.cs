using Finanzuebersicht.Services;

namespace Finanzuebersicht;

public partial class App : Application
{
	private readonly IDataService _dataService;

	public App(InitializationService initService, IDataService dataService)
	{
		InitializeComponent();
		_dataService = dataService;
		Task.Run(async () => await initService.InitializeAsync());
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());

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