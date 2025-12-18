using System.Drawing;

namespace LabelPrinting.Core;

/// <summary>
/// Defines the physical geometry of a label purely in millimeters.
/// </summary>
public sealed class LabelLayout
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LabelLayout"/> class.
    /// </summary>
    /// <param name="widthMm">The total width of the label in millimeters. Must be greater than 0.</param>
    /// <param name="heightMm">The total height of the label in millimeters. Must be greater than 0.</param>
    /// <param name="barcodeRectMm">The rectangle defining the barcode position and size in millimeters, relative to the label origin.</param>
    /// <param name="textRectMm">The rectangle defining the text area position and size in millimeters, relative to the label origin.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when widthMm or heightMm is less than or equal to 0.</exception>
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
