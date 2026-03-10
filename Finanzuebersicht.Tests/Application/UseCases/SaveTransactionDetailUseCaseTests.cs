using Finanzuebersicht.Application.UseCases.Transactions;
using Finanzuebersicht.Models;
using Finanzuebersicht.Services;
using NSubstitute;

namespace Finanzuebersicht.Tests.Application.UseCases;

public class SaveTransactionDetailUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesNewTransaction_WhenExistingIsNull()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var sut = new SaveTransactionDetailUseCase(transactionRepository);

        await sut.ExecuteAsync(
            existingTransaction: null,
            betrag: 120.50m,
            titel: "Einkauf",
            datum: new DateTime(2026, 3, 1),
            kategorieId: "cat-1",
            typ: TransactionType.Ausgabe,
            verwendungszweck: "Zweck A");

        await transactionRepository.Received(1).SaveTransactionAsync(
            Arg.Is<Transaction>(t =>
                t.Betrag == 120.50m &&
                t.Titel == "Einkauf" &&
                t.Datum == new DateTime(2026, 3, 1) &&
                t.KategorieId == "cat-1" &&
                t.Typ == TransactionType.Ausgabe &&
                t.Verwendungszweck == "Zweck A"));
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesExistingTransaction_WhenProvided()
    {
        var transactionRepository = Substitute.For<ITransactionRepository>();
        var existing = new Transaction
        {
            Id = "tx-1",
            Betrag = 10m,
            Titel = "Alt",
            KategorieId = "cat-old",
            Typ = TransactionType.Ausgabe,
            Datum = new DateTime(2026, 1, 1)
        };

        var sut = new SaveTransactionDetailUseCase(transactionRepository);

        await sut.ExecuteAsync(
            existingTransaction: existing,
            betrag: 200m,
            titel: "Neu",
            datum: new DateTime(2026, 3, 2),
            kategorieId: "cat-2",
            typ: TransactionType.Einnahme,
            verwendungszweck: "Gehaltszahlung");

        await transactionRepository.Received(1).SaveTransactionAsync(existing);
        Assert.Equal("tx-1", existing.Id);
        Assert.Equal(200m, existing.Betrag);
        Assert.Equal("Neu", existing.Titel);
        Assert.Equal(new DateTime(2026, 3, 2), existing.Datum);
        Assert.Equal("cat-2", existing.KategorieId);
        Assert.Equal(TransactionType.Einnahme, existing.Typ);
        Assert.Equal("Gehaltszahlung", existing.Verwendungszweck);
    }
}