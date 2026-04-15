using Finanzuebersicht.Services;

namespace Finanzuebersicht.Application.UseCases.SparZiele;

public class DeleteSparZielUseCase(ISparZielRepository sparZielRepository)
{
    public Task ExecuteAsync(string id)
        => sparZielRepository.DeleteSparZielAsync(id);
}
