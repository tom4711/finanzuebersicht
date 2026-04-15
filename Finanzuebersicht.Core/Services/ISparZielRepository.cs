using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public interface ISparZielRepository
{
    Task<List<SparZiel>> GetSparZieleAsync();
    Task SaveSparZielAsync(SparZiel sparZiel);
    Task DeleteSparZielAsync(string id);
}
