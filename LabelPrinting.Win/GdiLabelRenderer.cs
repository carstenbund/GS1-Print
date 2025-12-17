using System.Drawing;
using LabelPrinting.Core;
using ZXing;
using ZXing.Common;
using ZXing.Datamatrix;
using ZXing.Windows.Compatibility;

namespace LabelPrinting.Win;

/// <summary>
/// Renders a single label instance onto a GDI+ surface using millimeter geometry.
/// </summary>
public sealed class GdiLabelRenderer
{
    /// <summary>
    /// Draws a complete label including DataMatrix barcode and human-readable text onto the specified graphics surface.
    /// </summary>
    /// <param name="graphics">The GDI+ Graphics object to draw on. Must have PageUnit set to Millimeter.</param>
    /// <param name="slotMm">The rectangle in millimeters where the label should be drawn on the page.</param>
    /// <param name="layout">The layout definition specifying barcode and text positions within the label.</param>
    /// <param name="data">The label data to encode and render.</param>
    /// <exception cref="ArgumentNullException">Thrown when graphics, layout, or data is null.</exception>
    public void DrawLabel(Graphics graphics, RectangleF slotMm, LabelLayout layout, LabelData data)
    {
        if (graphics is null) throw new ArgumentNullException(nameof(graphics));
        if (layout is null) throw new ArgumentNullException(nameof(layout));
        if (data is null) throw new ArgumentNullException(nameof(data));

        var payload = Gs1PayloadBuilder.Build(data);
        using var barcodeBitmap = CreateBarcodeBitmap(payload);

        var barcodeRect = new RectangleF(
            slotMm.X + layout.BarcodeRectMm.X,
            slotMm.Y + layout.BarcodeRectMm.Y,
            layout.BarcodeRectMm.Width,
            layout.BarcodeRectMm.Height);

        graphics.DrawImage(barcodeBitmap, barcodeRect);

        var textRect = new RectangleF(
            slotMm.X + layout.TextRectMm.X,
            slotMm.Y + layout.TextRectMm.Y,
            layout.TextRectMm.Width,
            layout.TextRectMm.Height);

        using var font = new Font("Arial", 5f, FontStyle.Regular, GraphicsUnit.Point);
        using var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        var humanReadable = BuildHumanReadable(data);
        graphics.DrawString(humanReadable, font, Brushes.Black, textRect, format);
    }

    /// <summary>
    /// Builds a human-readable text representation of the label data with GS1 Application Identifiers.
    /// </summary>
    /// <param name="data">The label data to format.</param>
    /// <returns>A multi-line string with AI prefixes: (01) for GTIN, (10) for lot, (17) for expiry, and (11) for manufacture if present.</returns>
    private static string BuildHumanReadable(LabelData data)
    {
        var lines = new List<string>
        {
            $"(01) {data.Gtin}",
            $"(17) {data.Expiry:yyMMdd}",
            $"(10) {data.Lot}",
            $"EXP  {data.Expiry:yyMMdd}"
        };

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Creates a DataMatrix barcode bitmap from the given payload string.
    /// </summary>
    /// <param name="payload">The GS1 payload string to encode in the barcode.</param>
    /// <returns>A 200x200 pixel Bitmap containing the DataMatrix barcode with no margins.</returns>
    private static Bitmap CreateBarcodeBitmap(string payload)
    {
        var encodingOptions = new EncodingOptions
        {
            Width = 100,
            Height = 100,
            Margin = 0
        };

        var writer = new BarcodeWriter<Bitmap>
        {
            Format = BarcodeFormat.DATA_MATRIX,
            Options = encodingOptions,
            Renderer = new BitmapRenderer()
        };

        return writer.Write(payload);
    }
}
