using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.SparZiele;

public class LoadSparZieleUseCase(ISparZielRepository sparZielRepository)
{
    public async Task<List<SparZielSummary>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var ziele = await sparZielRepository.GetSparZieleAsync();
        return ziele.Select(z => new SparZielSummary { SparZiel = z }).ToList();
    }
}
