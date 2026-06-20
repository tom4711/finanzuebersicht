using Finanzuebersicht.Models;

namespace Finanzuebersicht.Resources.Strings;

/// <summary>
/// Resource key names for enum display labels.
/// </summary>
public static class EnumResourceKeys
{
    public static string GetAccountType(AccountType type) => type switch
    {
        AccountType.Girokonto => ResourceKeys.AccountType_Girokonto,
        AccountType.Tagesgeld => ResourceKeys.AccountType_Tagesgeld,
        AccountType.Kreditkarte => ResourceKeys.AccountType_Kreditkarte,
        AccountType.Bargeld => ResourceKeys.AccountType_Bargeld,
        AccountType.Depot => ResourceKeys.AccountType_Depot,
        AccountType.Sonstiges => ResourceKeys.AccountType_Sonstiges,
        _ => ResourceKeys.AccountType_Sonstiges
    };

    public static string GetTransactionType(TransactionType type) => type switch
    {
        TransactionType.Einnahme => ResourceKeys.Lbl_Einnahme,
        TransactionType.Ausgabe => ResourceKeys.Lbl_Ausgabe,
        _ => ResourceKeys.Lbl_Ausgabe
    };

    public static string GetRecurrenceInterval(RecurrenceInterval interval) => interval switch
    {
        RecurrenceInterval.Daily => ResourceKeys.RecurrenceInterval_Daily,
        RecurrenceInterval.Weekly => ResourceKeys.RecurrenceInterval_Weekly,
        RecurrenceInterval.Monthly => ResourceKeys.RecurrenceInterval_Monthly,
        RecurrenceInterval.Quarterly => ResourceKeys.RecurrenceInterval_Quarterly,
        RecurrenceInterval.Yearly => ResourceKeys.RecurrenceInterval_Yearly,
        _ => ResourceKeys.RecurrenceInterval_Monthly
    };
}
