using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.SparZiele;

public class SaveSparZielUseCase(ISparZielRepository sparZielRepository)
{
    public Task ExecuteAsync(SparZiel sparZiel)
        => sparZielRepository.SaveSparZielAsync(sparZiel);
}
