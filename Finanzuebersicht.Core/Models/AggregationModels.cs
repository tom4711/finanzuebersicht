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
