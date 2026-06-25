using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Finanzuebersicht.Models;
using Finanzuebersicht.Presentation;
using Finanzuebersicht.ViewModels;

namespace Finanzuebersicht.Tests.Presentation;

public class CurrencyDisplayRefreshTests
{
    [Fact]
    public void Rebind_RefreshesCollectionItems()
    {
        var collection = new ObservableCollection<string> { "a", "b" };

        CurrencyDisplayRefresh.Rebind(collection);

        Assert.Equal(2, collection.Count);
        Assert.Equal("a", collection[0]);
        Assert.Equal("b", collection[1]);
    }

    [Fact]
    public void RebindTransactionGroups_RecreatesGroupsWithSameTransactions()
    {
        var transaction = new Transaction { Betrag = 12.34m, Typ = TransactionType.Ausgabe };
        var groups = new ObservableCollection<TransactionGroup>
        {
            new(new DateTime(2026, 3, 1), [transaction], isMonthGroup: true)
        };

        CurrencyDisplayRefresh.RebindTransactionGroups(groups);

        Assert.Single(groups);
        Assert.True(groups[0].IsMonthGroup);
        Assert.Same(transaction, groups[0][0]);
    }

    [Fact]
    public void Registry_RefreshAll_InvokesRegisteredViewModels()
    {
        var viewModel = new TestCurrencyRefreshViewModel();
        CurrencyRefreshRegistry.Register(viewModel);

        CurrencyRefreshRegistry.RefreshAll();

        Assert.Equal(1, viewModel.RefreshCount);
    }

    private sealed class TestCurrencyRefreshViewModel : ObservableObject, ICurrencyRefreshViewModel
    {
        public int RefreshCount { get; private set; }

        public void RefreshCurrencyDisplay() => RefreshCount++;
    }
}
