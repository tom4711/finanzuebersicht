using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Finanzuebersicht.Models;

namespace Finanzuebersicht.Core.Services
{
    public class ImportService(
        IEnumerable<IStatementParser> parsers,
        ITransactionRepository transactionRepository,
        ILogger<ImportService> logger,
        ICategoryRepository? categoryRepository = null,
        CategorizationService? categorizationService = null)
    {
        private const double HistoricalConfidenceThreshold = 0.5;

        private readonly IEnumerable<IStatementParser> _parsers = parsers;
        private readonly ITransactionRepository _transactionRepository = transactionRepository;
        private readonly ILogger<ImportService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ICategoryRepository? _categoryRepository = categoryRepository;
        private readonly CategorizationService? _categorizationService = categorizationService;

        public async Task<ImportResult> ImportFromCsvAsync(
            Stream csvStream,
            string? accountId = null,
            CancellationToken cancellationToken = default)
        {
            var preview = await AnalyzeCsvAsync(csvStream, accountId, cancellationToken).ConfigureAwait(false);
            if (!preview.Success)
            {
                return new ImportResult { ErrorMessage = preview.ErrorMessage };
            }

            var previewDuplicates = preview.Rows
                .Where(r => r.Status == ImportPreviewRowStatus.Duplicate)
                .Select(r => CloneTransaction(r.Transaction))
                .ToList();

            var commitResult = await CommitImportAsync(preview, cancellationToken: cancellationToken).ConfigureAwait(false);
            var allDuplicates = previewDuplicates.Concat(commitResult.Duplicates).ToList();

            return new ImportResult
            {
                Imported = commitResult.Imported,
                Duplicates = allDuplicates,
                SkippedMalformed = preview.InvalidCount,
                SaveErrors = commitResult.SaveErrors,
                ErrorMessage = commitResult.ErrorMessage
            };
        }

        public async Task<ImportPreviewResult> AnalyzeCsvAsync(
            Stream csvStream,
            string? accountId = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _logger.LogInformation("ImportService: starting analysis (accountId={AccountId})", accountId ?? "(none)");

            var parseResult = await ParseDtosAsync(csvStream, cancellationToken).ConfigureAwait(false);
            if (parseResult.ErrorMessage is not null)
            {
                return new ImportPreviewResult { ErrorMessage = parseResult.ErrorMessage };
            }

            var dtos = parseResult.Dtos!;
            var categories = await LoadCategoriesAsync().ConfigureAwait(false);
            var existingInRange = await LoadExistingTransactionsForDtosAsync(dtos).ConfigureAwait(false);
            var historicalCategories = await BuildHistoricalCategoryMapAsync(dtos, categories, cancellationToken).ConfigureAwait(false);

            var rows = new List<ImportPreviewRow>();
            var batchTransactions = new List<Transaction>();

            for (var index = 0; index < dtos.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dto = dtos[index];

                if (dto is null || dto.Buchungsdatum == default)
                {
                    var placeholderTransaction = new Transaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        Betrag = 0m,
                        Datum = dto?.Buchungsdatum ?? default,
                        Titel = dto is not null ? BuildTitle(dto) : string.Empty,
                        Verwendungszweck = dto?.Verwendungszweck ?? string.Empty,
                        KategorieId = string.Empty,
                        Typ = TransactionType.Ausgabe,
                        AccountId = accountId ?? dto?.SourceAccountId ?? string.Empty
                    };

                    rows.Add(new ImportPreviewRow
                    {
                        SourceIndex = index,
                        IsIncluded = false,
                        Status = ImportPreviewRowStatus.Invalid,
                        StatusMessage = "Missing booking date",
                        Transaction = placeholderTransaction
                    });
                    continue;
                }

                var transaction = CreateTransaction(dto, accountId);
                if (ContainsDuplicate(existingInRange, transaction) || ContainsDuplicate(batchTransactions, transaction))
                {
                    rows.Add(new ImportPreviewRow
                    {
                        SourceIndex = index,
                        IsIncluded = false,
                        Status = ImportPreviewRowStatus.Duplicate,
                        StatusMessage = "Possible duplicate",
                        Transaction = transaction
                    });
                    continue;
                }

                transaction.KategorieId = await ResolveCategoryIdForAnalyzeAsync(
                    dto,
                    categories,
                    historicalCategories,
                    cancellationToken).ConfigureAwait(false);

                var status = string.IsNullOrWhiteSpace(transaction.KategorieId)
                    ? ImportPreviewRowStatus.Uncategorized
                    : ImportPreviewRowStatus.Ready;

                rows.Add(new ImportPreviewRow
                {
                    SourceIndex = index,
                    IsIncluded = true,
                    Status = status,
                    StatusMessage = status == ImportPreviewRowStatus.Uncategorized ? "Category unresolved" : null,
                    Transaction = transaction
                });

                batchTransactions.Add(transaction);
            }

            _logger.LogInformation(
                "ImportService: analysis finished — ready={Ready}, duplicates={Duplicates}, invalid={Invalid}, uncategorized={Uncategorized}",
                rows.Count(r => r.Status == ImportPreviewRowStatus.Ready),
                rows.Count(r => r.Status == ImportPreviewRowStatus.Duplicate),
                rows.Count(r => r.Status == ImportPreviewRowStatus.Invalid),
                rows.Count(r => r.Status == ImportPreviewRowStatus.Uncategorized));

            return new ImportPreviewResult { Rows = rows };
        }

        public async Task<ImportResult> CommitImportAsync(
            ImportPreviewResult preview,
            IEnumerable<string>? selectedRowIds = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(preview);
            cancellationToken.ThrowIfCancellationRequested();

            if (!preview.Success)
            {
                return new ImportResult { ErrorMessage = preview.ErrorMessage };
            }

            var selectedIds = selectedRowIds?.ToHashSet(StringComparer.Ordinal)
                ?? preview.Rows.Where(r => r.IsIncluded).Select(r => r.Id).ToHashSet(StringComparer.Ordinal);

            var rowsToCommit = preview.Rows
                .Where(r => selectedIds.Contains(r.Id) && r.IsIncluded && IsCommittableStatus(r.Status))
                .ToList();

            var existingInRange = await LoadExistingTransactionsForRowsAsync(rowsToCommit).ConfigureAwait(false);
            var imported = new List<Transaction>();
            var duplicates = new List<Transaction>();
            var saveErrors = new List<string>();
            var committedBatch = new List<Transaction>();
            string? uncategorizedCategoryId = null;

            foreach (var row in rowsToCommit)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var transaction = CloneTransaction(row.Transaction);
                if (string.IsNullOrWhiteSpace(transaction.KategorieId))
                {
                    uncategorizedCategoryId ??= await EnsureUncategorizedCategoryAsync(cancellationToken).ConfigureAwait(false);
                    transaction.KategorieId = uncategorizedCategoryId;
                }

                if (ContainsDuplicate(existingInRange, transaction) || ContainsDuplicate(committedBatch, transaction))
                {
                    duplicates.Add(transaction);
                    continue;
                }

                try
                {
                    await _transactionRepository.SaveTransactionAsync(transaction).ConfigureAwait(false);
                    imported.Add(transaction);
                    existingInRange.Add(transaction);
                    committedBatch.Add(transaction);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ImportService: failed to save transaction '{Title}' {Amount} on {Date}",
                        transaction.Titel, transaction.Betrag, transaction.Datum);
                    saveErrors.Add($"{transaction.Titel} ({transaction.Datum:dd.MM.yyyy}): {ex.Message}");
                }
            }

            return new ImportResult
            {
                Imported = imported,
                Duplicates = duplicates,
                SaveErrors = saveErrors
            };
        }

        private async Task<(List<TransactionDto>? Dtos, string? ErrorMessage)> ParseDtosAsync(
            Stream csvStream,
            CancellationToken cancellationToken)
        {
            byte[] buffer;
            try
            {
                using var ms = new MemoryStream();
                await csvStream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                buffer = ms.ToArray();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ImportService: failed to read input stream");
                return (null, $"Fehler beim Lesen der Datei: {ex.Message}");
            }

            var parserList = _parsers?.ToList() ?? [];
            List<TransactionDto>? dtosList = null;
            foreach (var parser in parserList)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var parserStream = new MemoryStream(buffer, writable: false);
                try
                {
                    dtosList = parser.Parse(parserStream)?.ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ImportService: parser {Parser} threw during Parse", parser.GetType().Name);
                    dtosList = null;
                    continue;
                }

                if (dtosList is { Count: > 0 })
                {
                    _logger.LogInformation("ImportService: parser {Parser} matched with {Count} records",
                        parser.GetType().Name, dtosList.Count);
                    return (dtosList, null);
                }

                dtosList = null;
            }

            _logger.LogInformation("ImportService: no parser matched or no records found");
            return (null, "Kein passender Parser gefunden oder keine Datensätze in der Datei.");
        }

        private async Task<List<Category>> LoadCategoriesAsync()
        {
            if (_categoryRepository is null)
                return [];

            try
            {
                return await _categoryRepository.GetCategoriesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ImportService: failed to load categories during analysis");
                return [];
            }
        }

        private async Task<Dictionary<string, Category>> BuildHistoricalCategoryMapAsync(
            IReadOnlyList<TransactionDto> dtos,
            IReadOnlyList<Category> categories,
            CancellationToken cancellationToken)
        {
            if (_categoryRepository is null || categories.Count == 0)
                return new Dictionary<string, Category>(StringComparer.Ordinal);

            var categoriesById = categories.ToDictionary(c => c.Id, c => c);
            var uncategorizedIds = categories
                .Where(IsUncategorizedCategory)
                .Select(c => c.Id)
                .ToHashSet(StringComparer.Ordinal);

            var payees = dtos
                .Select(GetPayee)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => Normalize(p!))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (payees.Count == 0)
                return new Dictionary<string, Category>(StringComparer.Ordinal);

            // Limit timeframe to recent period to avoid scanning entire DB
            var since = DateTime.Today.AddMonths(-24);
            List<Transaction> transactions;
            try
            {
                transactions = await _transactionRepository
                    .GetTransactionsAsync(since, DateTime.MaxValue)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ImportService: failed to load historical transactions for categorization");
                return new Dictionary<string, Category>(StringComparer.Ordinal);
            }

            // Build index by normalized title to reduce repeated scans
            var transactionsByKey = transactions
                .Select(t => new { Key = Normalize(t.Titel), Tx = t })
                .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                .GroupBy(x => x.Key)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Tx).ToList(), StringComparer.Ordinal);

            var result = new Dictionary<string, Category>(StringComparer.Ordinal);
            foreach (var normalizedPayee in payees)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var matchingTransactions = transactionsByKey
                    .Where(kv => kv.Key == normalizedPayee || kv.Key.Contains(normalizedPayee))
                    .SelectMany(kv => kv.Value)
                    .ToList();

                if (matchingTransactions.Count == 0)
                    continue;

                var categoryCounts = matchingTransactions
                    .Where(t => !string.IsNullOrWhiteSpace(t.KategorieId) && !uncategorizedIds.Contains(t.KategorieId))
                    .GroupBy(t => t.KategorieId)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                if (categoryCounts.Count == 0)
                    continue;

                var top = categoryCounts[0];
                var confidence = (double)top.Count / matchingTransactions.Count;
                if (confidence < HistoricalConfidenceThreshold)
                    continue;

                if (!categoriesById.TryGetValue(top.CategoryId, out var category))
                    continue;

                result[normalizedPayee] = category;
            }

            return result;
        }

        private async Task<string> ResolveCategoryIdForAnalyzeAsync(
            TransactionDto dto,
            IReadOnlyList<Category> categories,
            IReadOnlyDictionary<string, Category> historicalCategories,
            CancellationToken cancellationToken)
        {
            if (categories.Count == 0)
                return string.Empty;

            if (_categorizationService is not null)
            {
                var category = await _categorizationService.TryCategorizAsync(
                    dto,
                    categories,
                    allowFallback: false,
                    strategyFilter: strategy => strategy is not HistoricalCategorizationStrategy,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                if (category is not null)
                    return category.Id;
            }

            var payee = GetPayee(dto);
            if (string.IsNullOrWhiteSpace(payee))
                return string.Empty;

            return historicalCategories.TryGetValue(Normalize(payee), out var historicalCategory)
                ? historicalCategory.Id
                : string.Empty;
        }

        private async Task<List<Transaction>> LoadExistingTransactionsForDtosAsync(IReadOnlyList<TransactionDto> dtos)
        {
            var validDates = dtos
                .Where(d => d?.Buchungsdatum != null && d.Buchungsdatum != default)
                .Select(d => d!.Buchungsdatum.Date)
                .ToList();

            if (validDates.Count == 0)
                return [];

            try
            {
                return await _transactionRepository
                    .GetTransactionsAsync(validDates.Min().AddDays(-1), validDates.Max().AddDays(1))
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ImportService: failed to load existing transactions for duplicate check");
                return [];
            }
        }

        private async Task<List<Transaction>> LoadExistingTransactionsForRowsAsync(IReadOnlyList<ImportPreviewRow> rows)
        {
            var validDates = rows
                .Select(r => r.Transaction.Datum.Date)
                .ToList();

            if (validDates.Count == 0)
                return [];

            try
            {
                return await _transactionRepository
                    .GetTransactionsAsync(validDates.Min().AddDays(-1), validDates.Max().AddDays(1))
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ImportService: failed to reload transactions for commit duplicate check");
                return [];
            }
        }

        private async Task<string> EnsureUncategorizedCategoryAsync(CancellationToken cancellationToken)
        {
            if (_categoryRepository is null)
                return string.Empty;

            var categories = await _categoryRepository.GetCategoriesAsync().ConfigureAwait(false);
            var uncategorized = categories.FirstOrDefault(IsUncategorizedCategory);
            if (uncategorized is not null)
                return uncategorized.Id;

            uncategorized = new Category
            {
                Name = "Unkategorisiert",
                Icon = "❓",
                Color = "#A2845E",
                Typ = TransactionType.Ausgabe,
                SystemKey = Finanzuebersicht.Constants.SystemCategoryKeys.Unkategorisiert
            };

            cancellationToken.ThrowIfCancellationRequested();
            await _categoryRepository.SaveCategoryAsync(uncategorized).ConfigureAwait(false);
            return uncategorized.Id;
        }

        private static Transaction CreateTransaction(TransactionDto dto, string? accountId)
        {
            return new Transaction
            {
                // Normalize amount to positive value; sign represented by Typ
                Betrag = Math.Abs(dto.Betrag),
                Datum = dto.Buchungsdatum,
                Titel = BuildTitle(dto),
                Verwendungszweck = dto.Verwendungszweck ?? string.Empty,
                KategorieId = string.Empty,
                Typ = dto.Betrag >= 0 ? TransactionType.Einnahme : TransactionType.Ausgabe,
                AccountId = accountId ?? dto.SourceAccountId
            };
        }

        private static bool IsCommittableStatus(ImportPreviewRowStatus status)
            => status is ImportPreviewRowStatus.Ready or ImportPreviewRowStatus.Uncategorized or ImportPreviewRowStatus.SaveError;

        private static bool ContainsDuplicate(IEnumerable<Transaction> candidates, Transaction transaction)
            => candidates.Any(candidate => IsDuplicate(candidate, transaction));

        private static bool IsDuplicate(Transaction existing, Transaction candidate)
        {
            var from = candidate.Datum.Date.AddDays(-1);
            var to = candidate.Datum.Date.AddDays(1);
            return existing.Datum.Date >= from
                && existing.Datum.Date <= to
                && Math.Abs(existing.Betrag) == Math.Abs(candidate.Betrag)
                && Normalize(existing.Titel) == Normalize(candidate.Titel);
        }


        private static bool IsUncategorizedCategory(Category category)
            => category.SystemKey == Finanzuebersicht.Constants.SystemCategoryKeys.Unkategorisiert
               || string.Equals(category.Name, "Unkategorisiert", StringComparison.OrdinalIgnoreCase);

        private static string? GetPayee(TransactionDto dto)
        {
            return !string.IsNullOrWhiteSpace(dto.Zahlungsempfaenger)
                ? dto.Zahlungsempfaenger.Trim()
                : !string.IsNullOrWhiteSpace(dto.Zahlungspflichtige)
                    ? dto.Zahlungspflichtige.Trim()
                    : null;
        }

        private static Transaction CloneTransaction(Transaction transaction)
        {
            return new Transaction
            {
                Id = transaction.Id,
                Betrag = transaction.Betrag,
                Titel = transaction.Titel,
                Datum = transaction.Datum,
                KategorieId = transaction.KategorieId,
                Typ = transaction.Typ,
                DauerauftragId = transaction.DauerauftragId,
                AccountId = transaction.AccountId,
                Verwendungszweck = transaction.Verwendungszweck
            };
        }

        private static string BuildTitle(TransactionDto dto)
        {
            var title = !string.IsNullOrWhiteSpace(dto.Zahlungsempfaenger)
                ? dto.Zahlungsempfaenger
                : !string.IsNullOrWhiteSpace(dto.Zahlungspflichtige)
                    ? dto.Zahlungspflichtige
                    : dto.Verwendungszweck;

            return string.IsNullOrWhiteSpace(title)
                ? (string.IsNullOrWhiteSpace(dto.Verwendungszweck)
                    ? $"Buchung {dto.Buchungsdatum:dd.MM.yyyy} {dto.Betrag:0.00}€"
                    : dto.Verwendungszweck)
                : title;
        }

        private static string Normalize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var lowered = input.Trim().ToLowerInvariant();
            var normalized = lowered.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            var cleaned = new string([.. sb.ToString().Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))]);
            return Regex.Replace(cleaned, "\\s+", " ").Trim();
        }
    }
}
