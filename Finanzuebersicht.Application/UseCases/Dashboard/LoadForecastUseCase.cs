using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadForecastUseCase(IForecastService forecastService)
{
    public Task<ForecastResult> ExecuteAsync(int year, int month, int lookbackMonths = 3, string? accountId = null, CancellationToken cancellationToken = default)
        => forecastService.GetMovingAverageAsync(year, month, lookbackMonths, accountId);
}
