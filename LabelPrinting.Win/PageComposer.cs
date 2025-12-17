using System.Drawing;
using LabelPrinting.Core;

namespace LabelPrinting.Win;

/// <summary>
/// Calculates label slots on a physical page using millimeter geometry.
/// </summary>
public sealed class PageComposer
{
    private readonly float _marginMm;

    public PageComposer(float marginMm = 2f)
    {
        _marginMm = Math.Max(0, marginMm);
    }

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
