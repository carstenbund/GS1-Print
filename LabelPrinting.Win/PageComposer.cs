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
        var stepX = layout.WidthMm + _marginMm;
        var stepY = layout.HeightMm + _marginMm;

        for (float y = _marginMm; y + layout.HeightMm <= pageRectMm.Height; y += stepY)
        {
            for (float x = _marginMm; x + layout.WidthMm <= pageRectMm.Width; x += stepX)
            {
                slots.Add(new RectangleF(x, y, layout.WidthMm, layout.HeightMm));
            }
        }

        return slots;
    }
}
