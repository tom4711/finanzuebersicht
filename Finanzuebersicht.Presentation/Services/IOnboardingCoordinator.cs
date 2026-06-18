namespace Finanzuebersicht.Presentation.Services;

public interface IOnboardingCoordinator
{
    Task<bool> ShouldShowOnboardingAsync(CancellationToken cancellationToken = default);
    void MarkCompleted();
}
