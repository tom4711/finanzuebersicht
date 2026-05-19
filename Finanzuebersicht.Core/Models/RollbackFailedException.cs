namespace Finanzuebersicht.Models;

/// <summary>
/// Thrown when a rollback after a failed restore operation itself fails.
/// When this exception is caught, the data state may be inconsistent and
/// the user should be warned immediately.
/// </summary>
public class RollbackFailedException : Exception
{
    public RollbackFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}
