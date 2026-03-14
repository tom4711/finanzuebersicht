namespace Finanzuebersicht.Models;

public enum RecurringExceptionType
{
    Skip,
    Shift
}

public class RecurringException
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    // The date of the instance this exception applies to
    public DateTime InstanceDate { get; set; }
    public RecurringExceptionType Type { get; set; }
    // For Shift: new target date
    public DateTime? ShiftToDate { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
