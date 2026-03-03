using System.Globalization;

namespace Finanzuebersicht.Services;

public interface ITransactionValidationService
{
    bool TryValidate(
        string amountText,
        string title,
        bool hasCategory,
        CultureInfo culture,
        out decimal amount,
        out TransactionInputError? error);
}

public enum TransactionInputError
{
    InvalidAmountFormat,
    AmountMustBePositive,
    TitleRequired,
    CategoryRequired
}
