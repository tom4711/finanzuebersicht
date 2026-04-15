using System.Collections.Generic;

namespace Finanzuebersicht.Models
{
    public class CategorySummary
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        // Hex color string for UI/legend (e.g. "#FF5733"). Optional — LocalDataService fills it when available.
        public string? Color { get; set; }
        public string Icon { get; set; } = "📁";
        
        // For UI display: calculated percentage (0-100)
        public decimal PercentageAmount { get; set; }
        
        public string PercentageDisplay => $"{PercentageAmount:F1}%";

        // Budget fields (null if no budget set for this category)
        public decimal? BudgetBetrag { get; set; }
        public decimal? BudgetAbweichung => BudgetBetrag.HasValue ? Total - BudgetBetrag.Value : null;
        public bool HatBudget => BudgetBetrag.HasValue;
        public bool IstUeberBudget => BudgetAbweichung.HasValue && BudgetAbweichung.Value > 0;
        public decimal BudgetAusgeschoepft => BudgetBetrag.HasValue && BudgetBetrag.Value > 0
            ? Math.Min(Total / BudgetBetrag.Value * 100, 100)
            : 0;
    }

    public class ForecastResult
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal ForecastedTotal { get; set; }
        public List<CategorySummary> ByCategory { get; set; } = new();
        public int LookbackMonths { get; set; }
    }

    public class SparZielSummary
    {
        public SparZiel SparZiel { get; set; } = new();
        public decimal FortschrittProzent => SparZiel.ZielBetrag > 0
            ? Math.Min(SparZiel.AktuellerBetrag / SparZiel.ZielBetrag * 100, 100)
            : 0;
        public double FortschrittDecimal => (double)(FortschrittProzent / 100);
        public decimal FehlenderBetrag => Math.Max(SparZiel.ZielBetrag - SparZiel.AktuellerBetrag, 0);
    }

    public class MonthSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Total { get; set; }
        public List<CategorySummary> ByCategory { get; set; } = new();
    }

    public class YearSummary
    {
        public int Year { get; set; }
        public decimal Total { get; set; }
        public List<MonthSummary> Months { get; set; } = new();
        public List<CategorySummary> ByCategory { get; set; } = new();
    }
}
