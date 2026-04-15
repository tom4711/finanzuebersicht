using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.Dashboard;

public class LoadForecastUseCase(IForecastService forecastService)
{
    public Task<ForecastResult> ExecuteAsync(int year, int month, int lookbackMonths = 3)
        => forecastService.GetMovingAverageAsync(year, month, lookbackMonths);
}
