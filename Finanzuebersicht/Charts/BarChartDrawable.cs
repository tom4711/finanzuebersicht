using Finanzuebersicht.Models;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Finanzuebersicht.Charts;

/// <summary>
/// Zeichnet ein Balkendiagramm für den 12-Monats-Verlauf mit MAUI Graphics.
/// </summary>
public class BarChartDrawable : IDrawable
{
    private static readonly Color BarColor = Color.FromArgb("#007AFF");
    private static readonly Color BarColorLight = Color.FromArgb("#5AC8FA");
    private static readonly Color TextColor = Color.FromArgb("#8E8E93");
    private static readonly Color GridColor = Color.FromArgb("#E5E5EA");
    private static readonly Color HighlightColor = Color.FromArgb("#FF3B30");

    private static readonly string[] MonthLabels =
        ["J", "F", "M", "A", "M", "J", "J", "A", "S", "O", "N", "D"];

    public IReadOnlyList<MonthSummary> Months { get; set; } = [];
    public int CurrentMonth { get; set; } = Finanzuebersicht.Core.Services.SystemClock.Instance.Today.Month; // TODO: consider injecting IClock into drawable host (not straightforward from drawable)

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Months.Count == 0) return;

        canvas.Antialias = true;

        float paddingLeft = 8f;
        float paddingRight = 8f;
        float paddingTop = 12f;
        float paddingBottom = 22f;

        float chartWidth = dirtyRect.Width - paddingLeft - paddingRight;
        float chartHeight = dirtyRect.Height - paddingTop - paddingBottom;

        decimal maxVal = Months.Max(m => m.Total);
        if (maxVal <= 0) maxVal = 1;

        float barWidth = chartWidth / 12f;
        float barSpacing = barWidth * 0.25f;
        float actualBarWidth = barWidth - barSpacing;

        // Horizontale Gitternetzlinie
        canvas.StrokeColor = GridColor;
        canvas.StrokeSize = 1f;
        float baseY = paddingTop + chartHeight;
        canvas.DrawLine(paddingLeft, baseY, paddingLeft + chartWidth, baseY);

        for (int i = 0; i < 12; i++)
        {
            var month = Months.FirstOrDefault(m => m.Month == i + 1);
            decimal value = month?.Total ?? 0;

            float barHeight = chartHeight * (float)((double)value / (double)maxVal);
            float x = paddingLeft + i * barWidth + barSpacing / 2;
            float y = paddingTop + chartHeight - barHeight;

            // Balken
            bool isCurrent = (i + 1) == CurrentMonth;
            canvas.FillColor = isCurrent ? HighlightColor : BarColor;

            if (barHeight > 0)
            {
                var rect = new RectF(x, y, actualBarWidth, barHeight);
                canvas.FillRoundedRectangle(rect, 3);
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
