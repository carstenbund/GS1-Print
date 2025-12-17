using System.Drawing;
using LabelPrinting.Core;

namespace LabelPrinting.Win;

public static class DefaultLayouts
{
    /// <summary>
    /// Baseline 25x20mm label with a centered DataMatrix and text underneath.
    /// </summary>
    public static LabelLayout Gs1DataMatrixLabel { get; } = new(
        widthMm: 25f,
        heightMm: 20f,
        barcodeRectMm: new RectangleF(5.5f, 1.5f, 10f, 10f),
        textRectMm: new RectangleF(1.5f, 13f, 22f, 7f));
}
