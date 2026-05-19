using Finanzuebersicht.Models;

namespace Finanzuebersicht.Application.UseCases.Categories;

public class SaveCategoryBudgetUseCase(IBudgetRepository budgetRepository)
{
    public async Task ExecuteAsync(string kategorieId, decimal betrag, int? monat = null, int? jahr = null, CancellationToken cancellationToken = default)
    {
        var budgets = await budgetRepository.GetBudgetsAsync();
        var existing = budgets.FirstOrDefault(b =>
            b.KategorieId == kategorieId && b.Monat == monat && b.Jahr == jahr);

        if (betrag <= 0)
        {
            if (existing != null)
                await budgetRepository.DeleteBudgetAsync(existing.Id);
            return;
        }

        if (existing != null)
        {
            existing.Betrag = betrag;
            await budgetRepository.SaveBudgetAsync(existing);
        }
        else
        {
            await budgetRepository.SaveBudgetAsync(new CategoryBudget
            {
                KategorieId = kategorieId,
                Betrag = betrag,
                Monat = monat,
                Jahr = jahr
            });
        }
    }
}
