namespace Finanzuebersicht.Navigation;

/// <summary>
/// Type-safe route name constants for Shell navigation.
/// Replaces <c>nameof(XxxPage)</c> in ViewModels so they don't depend on View types.
/// </summary>
public static class Routes
{
    public static readonly string TransactionDetail = "TransactionDetailPage";
    public static readonly string RecurringTransactionDetail = "RecurringTransactionDetailPage";
    public static readonly string CategoryDetail = "CategoryDetailPage";
    public static readonly string RecurringInstanceShift = "RecurringInstanceShiftPage";
    public static readonly string Settings = "SettingsPage";
    public static readonly string BackupList = "BackupListPage";
}
