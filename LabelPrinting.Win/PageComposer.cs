using System.Drawing;
using LabelPrinting.Core;

namespace LabelPrinting.Win;

/// <summary>
/// Calculates label slots on a physical page using millimeter geometry.
/// </summary>
public sealed class PageComposer
{
    private readonly float _marginMm;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageComposer"/> class.
    /// </summary>
    /// <param name="marginMm">The margin spacing in millimeters between labels. Defaults to 2mm. Negative values are clamped to 0.</param>
    public PageComposer(float marginMm = 2f)
    {
        _marginMm = Math.Max(0, marginMm);
    }

    /// <summary>
    /// Calculates the positions where labels can be placed on a page, arranged in a grid.
    /// </summary>
    /// <param name="pageRectMm">The page dimensions in millimeters.</param>
    /// <param name="layout">The layout defining label size.</param>
    /// <returns>A read-only list of rectangles in millimeters, each representing a label slot position on the page. Labels are arranged in up to 2 columns, centered horizontally.</returns>
    public IReadOnlyList<RectangleF> GetLabelSlots(RectangleF pageRectMm, LabelLayout layout)
    {
        var slots = new List<RectangleF>();
        var stepY = layout.HeightMm + _marginMm;
        var maxColumns = Math.Max(1, (int)Math.Floor((pageRectMm.Width + _marginMm) / (layout.WidthMm + _marginMm)));
        var columns = Math.Min(2, maxColumns);
        var horizontalSpan = columns * layout.WidthMm + (columns - 1) * _marginMm;
        var startX = Math.Max(_marginMm, (pageRectMm.Width - horizontalSpan) / 2f);

        for (float y = _marginMm; y + layout.HeightMm <= pageRectMm.Height; y += stepY)
        {
            for (var column = 0; column < columns; column++)
            {
                var x = startX + column * (layout.WidthMm + _marginMm);
                if (x + layout.WidthMm > pageRectMm.Width)
                {
                    continue;
                }

                slots.Add(new RectangleF(x, y, layout.WidthMm, layout.HeightMm));
            }
        }

        return slots;
    }
}
