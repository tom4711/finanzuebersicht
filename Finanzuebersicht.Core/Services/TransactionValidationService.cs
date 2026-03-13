using System.Globalization;

namespace Finanzuebersicht.Services;

public class TransactionValidationService : ITransactionValidationService
{
    public bool TryValidate(
        string amountText,
        string title,
        bool hasCategory,
        CultureInfo culture,
        out decimal amount,
        out TransactionInputError? error)
    {
        error = null;

        if (!decimal.TryParse(amountText, NumberStyles.Any, culture, out amount))
        {
            error = TransactionInputError.InvalidAmountFormat;
            return false;
        }

        if (amount <= 0)
        {
            error = TransactionInputError.AmountMustBePositive;
            return false;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            error = TransactionInputError.TitleRequired;
            return false;
        }

        if (!hasCategory)
        {
            error = TransactionInputError.CategoryRequired;
            return false;
        }

        return true;
    }
}
