using CloudKit;
using Foundation;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Services;

public class CloudKitDataService : IDataService
{
    private const string RecordTypeCategory = "Category";
    private const string RecordTypeTransaction = "Transaction";
    private const string RecordTypeRecurring = "RecurringTransaction";

    private CKDatabase Database => CKContainer.DefaultContainer.PrivateCloudDatabase;

    #region Categories

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var records = await FetchRecordsAsync(RecordTypeCategory);
        return records.Select(MapToCategory).ToList();
    }

    public async Task SaveCategoryAsync(Category category)
    {
        var record = await FetchOrCreateRecordAsync(RecordTypeCategory, category.Id);
        record["name"] = (NSString)category.Name;
        record["icon"] = (NSString)category.Icon;
        record["color"] = (NSString)category.Color;
        record["typ"] = (NSNumber)(int)category.Typ;
        await SaveRecordAsync(record);
    }

    public async Task DeleteCategoryAsync(string id)
    {
        await DeleteRecordAsync(RecordTypeCategory, id);
    }

    #endregion

    #region Transactions

    public async Task<List<Transaction>> GetTransactionsAsync(DateTime vonDatum, DateTime bisDatum)
    {
        var predicate = NSPredicate.FromFormat(
            "datum >= %@ AND datum <= %@",
            DateToNSDate(vonDatum),
            DateToNSDate(bisDatum));

        var records = await FetchRecordsAsync(RecordTypeTransaction, predicate);
        return records.Select(MapToTransaction).OrderByDescending(t => t.Datum).ToList();
    }

    public async Task SaveTransactionAsync(Transaction transaction)
    {
        var record = await FetchOrCreateRecordAsync(RecordTypeTransaction, transaction.Id);
        record["betrag"] = (NSString)transaction.Betrag.ToString(System.Globalization.CultureInfo.InvariantCulture);
        record["titel"] = (NSString)transaction.Titel;
        record["datum"] = DateToNSDate(transaction.Datum);
        record["kategorieId"] = (NSString)transaction.KategorieId;
        record["typ"] = (NSNumber)(int)transaction.Typ;
        record["dauerauftragId"] = transaction.DauerauftragId != null
            ? (NSString)transaction.DauerauftragId
            : null!;
        await SaveRecordAsync(record);
    }

    public async Task DeleteTransactionAsync(string id)
    {
        await DeleteRecordAsync(RecordTypeTransaction, id);
    }

    #endregion

    #region Recurring Transactions

    public async Task<List<RecurringTransaction>> GetRecurringTransactionsAsync()
    {
        var records = await FetchRecordsAsync(RecordTypeRecurring);
        return records.Select(MapToRecurringTransaction).ToList();
    }

    public async Task SaveRecurringTransactionAsync(RecurringTransaction recurring)
    {
        var record = await FetchOrCreateRecordAsync(RecordTypeRecurring, recurring.Id);
        record["betrag"] = (NSString)recurring.Betrag.ToString(System.Globalization.CultureInfo.InvariantCulture);
        record["titel"] = (NSString)recurring.Titel;
        record["kategorieId"] = (NSString)recurring.KategorieId;
        record["typ"] = (NSNumber)(int)recurring.Typ;
        record["startdatum"] = DateToNSDate(recurring.Startdatum);
        record["enddatum"] = recurring.Enddatum.HasValue
            ? DateToNSDate(recurring.Enddatum.Value)
            : null!;
        record["aktiv"] = (NSNumber)(recurring.Aktiv ? 1 : 0);
        record["letzteAusfuehrung"] = DateToNSDate(recurring.LetzteAusfuehrung);
        await SaveRecordAsync(record);
    }

    public async Task DeleteRecurringTransactionAsync(string id)
    {
        await DeleteRecordAsync(RecordTypeRecurring, id);
    }

    public async Task GeneratePendingRecurringTransactionsAsync()
    {
        var dauerauftraege = await GetRecurringTransactionsAsync();
        var heute = DateTime.Today;

        foreach (var da in dauerauftraege.Where(d => d.Aktiv))
        {
            if (da.Enddatum.HasValue && da.Enddatum.Value < heute)
                continue;

            var naechsterMonat = da.LetzteAusfuehrung == default
                ? new DateTime(da.Startdatum.Year, da.Startdatum.Month, 1)
                : da.LetzteAusfuehrung.AddMonths(1);
            naechsterMonat = new DateTime(naechsterMonat.Year, naechsterMonat.Month, 1);

            while (naechsterMonat <= heute)
            {
                var transaction = new Transaction
                {
                    Betrag = da.Betrag,
                    Titel = da.Titel,
                    Datum = naechsterMonat,
                    KategorieId = da.KategorieId,
                    Typ = da.Typ,
                    DauerauftragId = da.Id
                };

                await SaveTransactionAsync(transaction);

                da.LetzteAusfuehrung = naechsterMonat;
                naechsterMonat = naechsterMonat.AddMonths(1);
            }

            await SaveRecurringTransactionAsync(da);
        }
    }

    #endregion

    #region CloudKit Helpers

    private async Task<List<CKRecord>> FetchRecordsAsync(string recordType, NSPredicate? predicate = null)
    {
        predicate ??= NSPredicate.FromFormat("TRUEPREDICATE");
        var query = new CKQuery(recordType, predicate);
        var results = new List<CKRecord>();

        try
        {
            var records = await Database.PerformQueryAsync(query, CKRecordZone.DefaultRecordZone().ZoneId);
            if (records != null)
            {
                results.AddRange(records);
            }
        }
        catch (NSErrorException ex) when (ex.Error.Code == (long)CKErrorCode.UnknownItem)
        {
            // Noch keine Records vorhanden
        }

        return results;
    }

    private async Task<CKRecord> FetchOrCreateRecordAsync(string recordType, string id)
    {
        var recordId = new CKRecordID(id);
        try
        {
            return await Database.FetchRecordAsync(recordId);
        }
        catch (NSErrorException)
        {
            return new CKRecord(recordType, recordId);
        }
    }

    private async Task SaveRecordAsync(CKRecord record)
    {
        await Database.SaveRecordAsync(record);
    }

    private async Task DeleteRecordAsync(string recordType, string id)
    {
        var recordId = new CKRecordID(id);
        try
        {
            await Database.DeleteRecordAsync(recordId);
        }
        catch (NSErrorException ex) when (ex.Error.Code == (long)CKErrorCode.UnknownItem)
        {
            // Record existiert nicht mehr – ignorieren
        }
    }

    #endregion

    #region Mapping

    private static Category MapToCategory(CKRecord record) => new()
    {
        Id = record.Id.RecordName,
        Name = record["name"]?.ToString() ?? string.Empty,
        Icon = record["icon"]?.ToString() ?? "💰",
        Color = record["color"]?.ToString() ?? "#007AFF",
        Typ = ParseTransactionType(record["typ"])
    };

    private static Transaction MapToTransaction(CKRecord record) => new()
    {
        Id = record.Id.RecordName,
        Betrag = ParseDecimal(record["betrag"]),
        Titel = record["titel"]?.ToString() ?? string.Empty,
        Datum = NSDateToDate(record["datum"] as NSDate),
        KategorieId = record["kategorieId"]?.ToString() ?? string.Empty,
        Typ = ParseTransactionType(record["typ"]),
        DauerauftragId = record["dauerauftragId"]?.ToString()
    };

    private static RecurringTransaction MapToRecurringTransaction(CKRecord record) => new()
    {
        Id = record.Id.RecordName,
        Betrag = ParseDecimal(record["betrag"]),
        Titel = record["titel"]?.ToString() ?? string.Empty,
        KategorieId = record["kategorieId"]?.ToString() ?? string.Empty,
        Typ = ParseTransactionType(record["typ"]),
        Startdatum = NSDateToDate(record["startdatum"] as NSDate),
        Enddatum = record["enddatum"] is NSDate end ? NSDateToDate(end) : null,
        Aktiv = (record["aktiv"] as NSNumber)?.Int32Value == 1,
        LetzteAusfuehrung = NSDateToDate(record["letzteAusfuehrung"] as NSDate)
    };

    private static decimal ParseDecimal(NSObject? value) =>
        decimal.TryParse(value?.ToString(), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result : 0m;

    private static TransactionType ParseTransactionType(NSObject? value) =>
        (value as NSNumber)?.Int32Value == (int)TransactionType.Einnahme
            ? TransactionType.Einnahme
            : TransactionType.Ausgabe;

    private static NSDate DateToNSDate(DateTime date) =>
        (NSDate)date.ToUniversalTime();

    private static DateTime NSDateToDate(NSDate? date) =>
        date != null ? (DateTime)date : DateTime.Today;

    #endregion
}
