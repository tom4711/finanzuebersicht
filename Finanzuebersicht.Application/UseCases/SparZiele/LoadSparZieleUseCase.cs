using Finanzuebersicht.Models;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.SparZiele;

public class LoadSparZieleUseCase(ISparZielRepository sparZielRepository)
{
    public async Task<List<SparZielSummary>> ExecuteAsync()
    {
        var ziele = await sparZielRepository.GetSparZieleAsync();
        return ziele.Select(z => new SparZielSummary { SparZiel = z }).ToList();
    }
}
