namespace Finanzuebersicht.Services;

/// <summary>
/// Abstracts MainThread.InvokeOnMainThreadAsync so ViewModels remain testable
/// without a MAUI runtime.
/// </summary>
public interface IMainThreadDispatcher
{
    Task InvokeAsync(Func<Task> action);
}
