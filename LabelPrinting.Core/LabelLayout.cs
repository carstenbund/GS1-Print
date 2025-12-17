using System.Drawing;

namespace LabelPrinting.Core;

/// <summary>
/// Defines the physical geometry of a label purely in millimeters.
/// </summary>
public sealed class LabelLayout
{
    public LabelLayout(float widthMm, float heightMm, RectangleF barcodeRectMm, RectangleF textRectMm)
    {
        if (widthMm <= 0) throw new ArgumentOutOfRangeException(nameof(widthMm));
        if (heightMm <= 0) throw new ArgumentOutOfRangeException(nameof(heightMm));

        WidthMm = widthMm;
        HeightMm = heightMm;
        BarcodeRectMm = barcodeRectMm;
        TextRectMm = textRectMm;
    }

    public float WidthMm { get; }

    public float HeightMm { get; }

    public RectangleF BarcodeRectMm { get; }

    public RectangleF TextRectMm { get; }
}
