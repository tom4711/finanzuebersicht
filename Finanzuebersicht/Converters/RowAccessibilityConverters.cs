using System.Globalization;
using Finanzuebersicht.Core.Services;
using Finanzuebersicht.Models;
using Finanzuebersicht.Resources.Strings;
using Finanzuebersicht.Services;

namespace Finanzuebersicht.Converters;

public class TransactionRowAccessibilityConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 1 || values[0] is not Transaction tx)
            return string.Empty;

        IReadOnlyDictionary<string, string>? categoryMap = values.Length >= 2
            ? values[1] as IReadOnlyDictionary<string, string>
            : null;
        IReadOnlyDictionary<string, string>? accountMap = values.Length >= 3
            ? values[2] as IReadOnlyDictionary<string, string>
            : null;

        var amount = FormatTransactionAmount(tx);
        var account = ResolveName(accountMap, tx.AccountId, LocalizationResourceManager.Current[ResourceKeys.Lbl_AlleKonten]);
        var category = ResolveName(categoryMap, tx.KategorieId, LocalizationResourceManager.Current[ResourceKeys.Lbl_OhneKategorie]);
        var date = tx.Datum.ToString("d", culture);

        return string.Format(
            LocalizationResourceManager.Current[ResourceKeys.A11y_TransaktionZeile],
            tx.Titel,
            amount,
            category,
            account,
            date);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static string FormatTransactionAmount(Transaction tx)
    {
        var ci = CurrencyCulture.Instance;
        var abs = Math.Abs(tx.Betrag);
        var formatted = abs.ToString("C", ci);
        var sign = tx.Typ == TransactionType.Einnahme ? "+" : "-";
        return $"{sign}{formatted}";
    }

    private static string ResolveName(IReadOnlyDictionary<string, string>? map, string? id, string fallback)
    {
        if (string.IsNullOrWhiteSpace(id))
            return fallback;

        return map is not null && map.TryGetValue(id, out var name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : fallback;
    }
}

public class CategoryRowAccessibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Category category)
            return string.Empty;

        var typeText = LocalizationResourceManager.Current[EnumResourceKeys.GetTransactionType(category.Typ)];
        return string.Format(
            LocalizationResourceManager.Current[ResourceKeys.A11y_KategorieZeile],
            category.Name,
            typeText);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class AccountRowAccessibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AccountListItem item)
            return string.Empty;

        var typeText = LocalizationResourceManager.Current[EnumResourceKeys.GetAccountType(item.Type)];
        var saldo = item.Saldo.ToString("C", CurrencyCulture.Instance);
        var status = item.IsArchived
            ? LocalizationResourceManager.Current[ResourceKeys.Lbl_Archiviert]
            : LocalizationResourceManager.Current[ResourceKeys.Status_Aktiv];

        return string.Format(
            LocalizationResourceManager.Current[ResourceKeys.A11y_KontoZeile],
            item.Name,
            typeText,
            saldo,
            status);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class RecurringRowAccessibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not RecurringTransaction recurring)
            return string.Empty;

        var amount = TransactionRowAccessibilityConverter.FormatTransactionAmount(new Transaction
        {
            Betrag = recurring.Betrag,
            Typ = recurring.Typ
        });
        var typeText = LocalizationResourceManager.Current[EnumResourceKeys.GetTransactionType(recurring.Typ)];
        var status = recurring.Aktiv
            ? LocalizationResourceManager.Current[ResourceKeys.Status_Aktiv]
            : LocalizationResourceManager.Current[ResourceKeys.Status_Inaktiv];

        return string.Format(
            LocalizationResourceManager.Current[ResourceKeys.A11y_DauerauftragZeile],
            recurring.Titel,
            amount,
            typeText,
            status);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class SparZielRowAccessibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SparZielSummary summary)
            return string.Empty;

        var ci = CurrencyCulture.Instance;
        return string.Format(
            LocalizationResourceManager.Current[ResourceKeys.A11y_SparZielZeile],
            summary.SparZiel.Titel,
            summary.GesamtFortschritt.ToString("C", ci),
            summary.SparZiel.ZielBetrag.ToString("C", ci),
            summary.FortschrittProzent.ToString("F0", culture));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
