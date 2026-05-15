namespace Finanzuebersicht.Navigation;

/// <summary>
/// Type-safe route name constants for Shell navigation.
/// Replaces <c>nameof(XxxPage)</c> in ViewModels so they don't depend on View types.
/// </summary>
public static class Routes
{
    public const string TransactionDetail = "TransactionDetailPage";
    public const string RecurringTransactionDetail = "RecurringTransactionDetailPage";
    public const string CategoryDetail = "CategoryDetailPage";
    public const string RecurringInstanceShift = "RecurringInstanceShiftPage";
    public const string Settings = "SettingsPage";
    public const string BackupList = "BackupListPage";
}
