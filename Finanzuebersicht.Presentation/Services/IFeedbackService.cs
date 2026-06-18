namespace Finanzuebersicht.Presentation.Services;

public interface IFeedbackService
{
    Task ShowSnackbarAsync(string message, string? actionText = null, Func<Task>? onActionAsync = null);
}
