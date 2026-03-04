using Finanzuebersicht.Converters;
using Finanzuebersicht.Models;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Finanzuebersicht.Charts;

/// <summary>
/// Zeichnet ein Donut-Diagramm für Kategorie-Ausgaben mit MAUI Graphics.
/// Segmente werden als Polygone approximiert (vermeidet PathF.AddArc-Bugs).
/// </summary>
public class DonutChartDrawable : IDrawable
{
    private const int ArcSteps = 60;
    private static readonly Color FallbackColor = ColorResourceHelper.GetColor("Primary", Color.FromArgb("#007AFF"));

    // Theme-abhängige Farben werden zur Laufzeit ausgelesen
    private static Color TextColor =>
        ColorResourceHelper.GetThemeColor(
            "Gray900",
            "Gray100",
            Color.FromArgb("#1C1C1E"),
            Color.FromArgb("#F2F2F7"));

    private static Color SecondaryTextColor =>
        ColorResourceHelper.GetThemeColor(
            "Gray600",
            "Gray600",
            Color.FromArgb("#6C6C70"),
            Color.FromArgb("#8E8E93"));

    private static Color HoleColor =>
        ColorResourceHelper.GetThemeColor(
            "PageBackground",
            "PageBackgroundDark",
            Color.FromArgb("#F2F2F7"),
            Color.FromArgb("#1C1C1E"));

    public IReadOnlyList<CategorySummary> Items { get; set; } = [];

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var items = Items.Where(i => i.Total > 0).ToList();
        if (items.Count == 0) return;

        canvas.Antialias = true;

        // Layout: Donut links, Legende rechts
        float donutSize = Math.Min(dirtyRect.Height, dirtyRect.Width * 0.45f);
        float cx = donutSize / 2f;
        float cy = dirtyRect.Height / 2f;
        float outerR = donutSize * 0.44f;
        float innerR = outerR * 0.55f;

        // --- Segmente als Polygon-Annäherung zeichnen ---
        decimal total = items.Sum(i => i.Total);
        float startDeg = -90f;

        foreach (var item in items)
        {
            float sweep = (float)((double)item.Total / (double)total * 360.0);
            canvas.FillColor = ParseColor(item.Color);
            canvas.FillPath(BuildWedge(cx, cy, outerR, startDeg, sweep));
            startDeg += sweep;
        }

        // --- Donut-Loch ---
        canvas.FillColor = HoleColor;
        canvas.FillCircle(cx, cy, innerR);

        // --- Legende rechts ---
        float legendX = donutSize + 12f;
        float availWidth = dirtyRect.Width - legendX - 4f;
        if (availWidth < 60) return;

        float rowH = Math.Min(22f, dirtyRect.Height / Math.Max(items.Count, 1));
        float dotR = Math.Max(4f, rowH * 0.25f);
        float fontSize = Math.Max(11f, rowH * 0.58f);
        float totalLegendH = items.Count * rowH;
        float legendY = (dirtyRect.Height - totalLegendH) / 2f;

        foreach (var item in items)
        {
            float midY = legendY + rowH / 2f;

            // Farbpunkt
            canvas.FillColor = ParseColor(item.Color);
            canvas.FillCircle(legendX + dotR, midY, dotR);

            float textX = legendX + dotR * 2 + 5f;
            float textW = availWidth - dotR * 2 - 5f;

            // Kategoriename
            canvas.FontSize = fontSize;
            canvas.FontColor = TextColor;
            canvas.DrawString(
                item.CategoryName,
                textX, legendY, textW, rowH,
                HorizontalAlignment.Left, VerticalAlignment.Center);

            // Prozentzahl
            if (total > 0)
            {
                string pct = $"{(double)item.Total / (double)total:P0}";
                canvas.FontSize = Math.Max(10f, fontSize - 2f);
                canvas.FontColor = SecondaryTextColor;
                canvas.DrawString(pct,
                    textX, legendY, textW - 2f, rowH,
                    HorizontalAlignment.Right, VerticalAlignment.Center);
            }

            legendY += rowH;
        }
    }

    /// <summary>Baut einen Tortenschnitt als PathF aus Liniensegmenten.</summary>
    private static PathF BuildWedge(float cx, float cy, float r, float startDeg, float sweepDeg)
    {
        var path = new PathF();
        path.MoveTo(cx, cy);

        int steps = Math.Max(3, (int)(ArcSteps * sweepDeg / 360f));
        for (int i = 0; i <= steps; i++)
        {
            float deg = startDeg + sweepDeg * i / steps;
            float rad = DegToRad(deg);
            path.LineTo(cx + r * MathF.Cos(rad), cy + r * MathF.Sin(rad));
        }

        path.Close();
        return path;
    }

    private static float DegToRad(float deg) => deg * MathF.PI / 180f;

    private static Color ParseColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return FallbackColor;
        try { return Color.FromArgb(hex); }
        catch { return FallbackColor; }
    }
}

