using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Finanzuebersicht.Presentation.Services;

namespace Finanzuebersicht.Services;

public class MauiFeedbackService(IMainThreadDispatcher dispatcher) : IFeedbackService
{
    private readonly IMainThreadDispatcher _dispatcher = dispatcher;
    private static readonly TimeSpan SnackbarDuration = TimeSpan.FromSeconds(5);

    public Task ShowSnackbarAsync(string message, string? actionText = null, Func<Task>? onActionAsync = null)
        => _dispatcher.InvokeAsync(async () =>
        {
            var snackbar = string.IsNullOrWhiteSpace(actionText) || onActionAsync is null
                ? Snackbar.Make(message, duration: SnackbarDuration)
                : Snackbar.Make(message, async () => await onActionAsync(), actionText, duration: SnackbarDuration);

            await snackbar.Show();
        });
}
