using Finanzuebersicht.Converters;
using Finanzuebersicht.Models;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Finanzuebersicht.Charts;

/// <summary>
/// Zeichnet ein Balkendiagramm für den 12-Monats-Verlauf mit MAUI Graphics.
/// Unterstützt Forecast-Balken (gestrichelt) und Budget-Referenzlinie.
/// </summary>
public class BarChartDrawable : IDrawable
{
    private static readonly string[] MonthLabels =
        ["J", "F", "M", "A", "M", "J", "J", "A", "S", "O", "N", "D"];

    private static Color BarColor =>
        ColorResourceHelper.GetThemeColor("Primary", "Primary",
            Color.FromArgb("#007AFF"), Color.FromArgb("#0A84FF"));

    private static Color BarColorForecast =>
        ColorResourceHelper.GetThemeColor("Primary", "Primary",
            Color.FromArgb("#007AFF"), Color.FromArgb("#0A84FF"))
        .WithAlpha(0.35f);

    private static Color HighlightColor =>
        Color.FromArgb("#FF3B30");

    private static Color TextColor =>
        ColorResourceHelper.GetThemeColor("Gray600", "Gray400",
            Color.FromArgb("#8E8E93"), Color.FromArgb("#AEAEB2"));

    private static Color GridColor =>
        ColorResourceHelper.GetThemeColor("Gray200", "Gray700",
            Color.FromArgb("#E5E5EA"), Color.FromArgb("#3A3A3C"));

    private static Color BudgetLineColor =>
        ColorResourceHelper.GetThemeColor("Gray500", "Gray400",
            Color.FromArgb("#AEAEB2"), Color.FromArgb("#636366"));

    public IReadOnlyList<MonthSummary> Months { get; set; } = [];
    public int CurrentMonth { get; set; } = Finanzuebersicht.Core.Services.SystemClock.Instance.Today.Month;

    /// <summary>Monat (1–12) für den Forecast-Balken, 0 = kein Forecast.</summary>
    public int ForecastMonth { get; set; }

    /// <summary>Forecast-Betrag für den Forecast-Balken.</summary>
    public decimal ForecastValue { get; set; }

    /// <summary>Monatliches Gesamtbudget als horizontale Referenzlinie, 0 = keine Linie.</summary>
    public decimal MonthlyBudgetTotal { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Months.Count == 0 && ForecastMonth == 0) return;

        canvas.Antialias = true;

        float paddingLeft = 8f;
        float paddingRight = 8f;
        float paddingTop = 12f;
        float paddingBottom = 22f;

        float chartWidth = dirtyRect.Width - paddingLeft - paddingRight;
        float chartHeight = dirtyRect.Height - paddingTop - paddingBottom;

        // Max-Wert berücksichtigt Forecast und Budget für korrekte Skalierung
        decimal maxVal = Months.Count > 0 ? Months.Max(m => m.Total) : 0;
        if (ForecastMonth > 0) maxVal = Math.Max(maxVal, ForecastValue);
        if (MonthlyBudgetTotal > 0) maxVal = Math.Max(maxVal, MonthlyBudgetTotal);
        if (maxVal <= 0) maxVal = 1;

        float barWidth = chartWidth / 12f;
        float barSpacing = barWidth * 0.25f;
        float actualBarWidth = barWidth - barSpacing;
        float baseY = paddingTop + chartHeight;

        // Horizontale Basislinie
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        canvas.StrokeDashPattern = null;
        canvas.DrawLine(paddingLeft, baseY, paddingLeft + chartWidth, baseY);

        // Budget-Referenzlinie (gestrichelt)
        if (MonthlyBudgetTotal > 0)
        {
            float budgetY = paddingTop + chartHeight * (1f - (float)((double)MonthlyBudgetTotal / (double)maxVal));
            canvas.StrokeColor = BudgetLineColor;
            canvas.StrokeSize = 1.5f;
            canvas.StrokeDashPattern = [4f, 4f];
            canvas.DrawLine(paddingLeft, budgetY, paddingLeft + chartWidth, budgetY);
            canvas.StrokeDashPattern = null;
        }

        for (int i = 0; i < 12; i++)
        {
            var month = Months.FirstOrDefault(m => m.Month == i + 1);
            bool isForecast = (i + 1) == ForecastMonth && ForecastValue > 0 && (month?.Total ?? 0) == 0;
            decimal value = isForecast ? ForecastValue : (month?.Total ?? 0);

            float x = paddingLeft + i * barWidth + barSpacing / 2;
            bool isCurrent = (i + 1) == CurrentMonth;

            if (value > 0)
            {
                float barHeight = chartHeight * (float)((double)value / (double)maxVal);
                float y = paddingTop + chartHeight - barHeight;
                var rect = new RectF(x, y, actualBarWidth, barHeight);

                if (isForecast)
                    canvas.FillColor = BarColorForecast;
                else if (isCurrent)
                    canvas.FillColor = HighlightColor;
                else
                    canvas.FillColor = BarColor;

                canvas.FillRoundedRectangle(rect, 3);

                // Forecast-Tilde-Symbol oben
                if (isForecast)
                {
                    canvas.FontColor = BarColor;
                    canvas.FontSize = 9f;
                    canvas.DrawString("~", x, y - 13, actualBarWidth, 13,
                        HorizontalAlignment.Center, VerticalAlignment.Bottom);
                }
            }

            // Monatskürzel
            canvas.FontColor = isCurrent ? HighlightColor : TextColor;
            canvas.FontSize = 10f;
            canvas.DrawString(MonthLabels[i],
                x, baseY + 3, actualBarWidth, paddingBottom - 4,
                HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }
}
