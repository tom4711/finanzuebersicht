using Finanzuebersicht.Models;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Finanzuebersicht.Charts;

/// <summary>
/// Zeichnet ein Donut-Diagramm für Kategorie-Ausgaben mit MAUI Graphics.
/// </summary>
public class DonutChartDrawable : IDrawable
{
    private static readonly Color FallbackColor = Color.FromArgb("#007AFF");
    private static readonly Color TextColor = Color.FromArgb("#1C1C1E");
    private static readonly Color SecondaryTextColor = Color.FromArgb("#8E8E93");
    private static readonly Color BackgroundColor = Color.FromArgb("#F2F2F7");

    public IReadOnlyList<CategorySummary> Items { get; set; } = [];
    public string CenterLabel { get; set; } = string.Empty;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var items = Items.Where(i => i.Total > 0).ToList();
        if (items.Count == 0)
            return;

        canvas.Antialias = true;

        float cx = dirtyRect.Width / 2f;
        float cy = dirtyRect.Height / 2f;
        float radius = Math.Min(cx, cy) * 0.82f;
        float innerRadius = radius * 0.56f;

        decimal total = items.Sum(i => i.Total);
        float startAngle = -90f;

        // Segmente zeichnen
        foreach (var item in items)
        {
            float sweep = (float)((double)item.Total / (double)total * 360.0);
            canvas.FillColor = ParseColor(item.Color);

            var path = new PathF();
            AddDonutSegment(path, cx, cy, radius, innerRadius, startAngle, sweep);
            canvas.FillPath(path);

            startAngle += sweep;
        }

        // Inneren Kreis (Loch) mit Hintergrundfarbe füllen
        canvas.FillColor = BackgroundColor;
        canvas.FillCircle(cx, cy, innerRadius - 1);

        // Mittig: Gesamtbetrag
        if (!string.IsNullOrEmpty(CenterLabel))
        {
            canvas.FontColor = TextColor;
            canvas.FontSize = 13f;
            canvas.DrawString(CenterLabel, cx - 45, cy - 10, 90, 20,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        // Legende rechts oder unten
        DrawLegend(canvas, dirtyRect, items, total);
    }

    private static void AddDonutSegment(PathF path, float cx, float cy,
        float outerR, float innerR, float startDeg, float sweepDeg)
    {
        float startRad = DegToRad(startDeg);
        float endRad = DegToRad(startDeg + sweepDeg);

        float ox1 = cx + outerR * MathF.Cos(startRad);
        float oy1 = cy + outerR * MathF.Sin(startRad);
        float ox2 = cx + outerR * MathF.Cos(endRad);
        float oy2 = cy + outerR * MathF.Sin(endRad);
        float ix1 = cx + innerR * MathF.Cos(endRad);
        float iy1 = cy + innerR * MathF.Sin(endRad);
        float ix2 = cx + innerR * MathF.Cos(startRad);
        float iy2 = cy + innerR * MathF.Sin(startRad);

        path.MoveTo(ox1, oy1);
        path.AddArc(cx - outerR, cy - outerR, cx + outerR, cy + outerR,
            startDeg, startDeg + sweepDeg, false);
        path.LineTo(ix1, iy1);
        path.AddArc(cx - innerR, cy - innerR, cx + innerR, cy + innerR,
            startDeg + sweepDeg, startDeg, true);
        path.LineTo(ox1, oy1);
        path.Close();
    }

    private void DrawLegend(ICanvas canvas, RectF rect,
        List<CategorySummary> items, decimal total)
    {
        float legendX = rect.Width * 0.58f;
        float legendY = rect.Height * 0.08f;
        float rowHeight = Math.Min(18f, (rect.Height * 0.85f) / Math.Max(items.Count, 1));
        float dotSize = rowHeight * 0.5f;

        foreach (var item in items)
        {
            if (legendY + rowHeight > rect.Height - 4) break;

            // Farbpunkt
            canvas.FillColor = ParseColor(item.Color);
            canvas.FillCircle(legendX + dotSize / 2, legendY + rowHeight / 2, dotSize / 2);

            // Name
            canvas.FontColor = TextColor;
            canvas.FontSize = rowHeight * 0.6f;
            canvas.DrawString(
                TruncateName(item.CategoryName, 12),
                legendX + dotSize + 4, legendY,
                rect.Width - legendX - dotSize - 8, rowHeight,
                HorizontalAlignment.Left, VerticalAlignment.Center);

            // Prozent
            canvas.FontColor = SecondaryTextColor;
            canvas.FontSize = rowHeight * 0.55f;
            string pct = total > 0 ? $"{(double)item.Total / (double)total:P0}" : "";
            canvas.DrawString(pct,
                legendX + dotSize + 4, legendY,
                rect.Width - legendX - dotSize - 12, rowHeight,
                HorizontalAlignment.Right, VerticalAlignment.Center);

            legendY += rowHeight + 2;
        }
    }

    private static float DegToRad(float deg) => deg * MathF.PI / 180f;

    private static Color ParseColor(string? hex)
    {
        try { return Color.FromArgb(hex ?? "#007AFF"); }
        catch { return FallbackColor; }
    }

    private static string TruncateName(string name, int max) =>
        name.Length > max ? name[..max] + "…" : name;
}
