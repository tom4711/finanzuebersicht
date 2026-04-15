using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface IForecastService
{
    /// <summary>Moving average of the last N months ending before the given year/month.</summary>
    Task<ForecastResult> GetMovingAverageAsync(int year, int month, int lookbackMonths = 3);
}
