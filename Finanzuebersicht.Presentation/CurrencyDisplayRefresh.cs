using System.Collections.ObjectModel;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Presentation;

public static class CurrencyDisplayRefresh
{
    public static void Rebind<T>(ObservableCollection<T> collection)
    {
        if (collection.Count == 0)
            return;

        var snapshot = collection.ToList();
        collection.Clear();
        foreach (var item in snapshot)
            collection.Add(item);
    }

    public static ObservableCollection<T> Clone<T>(ObservableCollection<T> collection) =>
        new(collection);

    public static void RebindTransactionGroups(ObservableCollection<TransactionGroup> groups)
    {
        if (groups.Count == 0)
            return;

        var rebuilt = groups
            .Select(g => new TransactionGroup(g.Datum, g.ToList(), g.IsMonthGroup))
            .ToList();
        groups.Clear();
        foreach (var group in rebuilt)
            groups.Add(group);
    }
}
