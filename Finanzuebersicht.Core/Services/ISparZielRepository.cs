using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services;

public interface ISparZielRepository
{
    Task<List<SparZiel>> GetSparZieleAsync();
    Task SaveSparZielAsync(SparZiel sparZiel);
    Task DeleteSparZielAsync(string id);
    Task ReplaceAllSparZieleAsync(IEnumerable<SparZiel> sparziele);
}
