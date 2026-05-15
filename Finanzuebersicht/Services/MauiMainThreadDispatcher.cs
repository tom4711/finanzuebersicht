namespace Finanzuebersicht.Services;

/// <summary>
/// MAUI implementation: dispatches work onto the main/UI thread.
/// </summary>
public class MauiMainThreadDispatcher : IMainThreadDispatcher
{
    public Task InvokeAsync(Func<Task> action)
        => MainThread.InvokeOnMainThreadAsync(action);
}
