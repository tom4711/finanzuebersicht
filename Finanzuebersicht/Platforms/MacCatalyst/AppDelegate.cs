using Foundation;
using UIKit;

namespace Finanzuebersicht;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

	// Mac Catalyst: Fenster schließen beendet die App nicht (sonst wirkt sie „einfach weg“)
	[Export("applicationShouldTerminateAfterLastWindowClosed:")]
	public bool ApplicationShouldTerminateAfterLastWindowClosed(UIApplication application) => false;
}
