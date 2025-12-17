using System.Drawing;
using LabelPrinting.Core;

namespace LabelPrinting.Win;

public static class DefaultLayouts
{
    /// <summary>
    /// Baseline 25x20mm label with square DataMatrix and stacked text.
    /// </summary>
    public static LabelLayout Gs1DataMatrixLabel { get; } = new(
        widthMm: 25f,
        heightMm: 20f,
        barcodeRectMm: new RectangleF(1.5f, 1.5f, 16f, 16f),
        textRectMm: new RectangleF(18f, 1.5f, 5.5f, 16f));
}
