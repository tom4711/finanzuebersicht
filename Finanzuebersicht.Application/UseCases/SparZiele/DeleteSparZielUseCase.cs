
namespace Finanzuebersicht.Application.UseCases.SparZiele;

public class DeleteSparZielUseCase(ISparZielRepository sparZielRepository)
{
    public Task ExecuteAsync(string id, CancellationToken cancellationToken = default)
        => sparZielRepository.DeleteSparZielAsync(id);
}
