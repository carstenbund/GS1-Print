using System.Drawing;
using LabelPrinting.Core;
using ZXing;
using ZXing.Common;
using ZXing.Datamatrix;
using ZXing.Rendering;

namespace LabelPrinting.Win;

/// <summary>
/// Renders a single label instance onto a GDI+ surface using millimeter geometry.
/// </summary>
public sealed class GdiLabelRenderer
{
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

        using var font = new Font("Arial", 5.5f, FontStyle.Regular, GraphicsUnit.Point);
        using var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        var humanReadable = BuildHumanReadable(data);
        graphics.DrawString(humanReadable, font, Brushes.Black, textRect, format);
    }

    private static string BuildHumanReadable(LabelData data)
    {
        var lines = new List<string>
        {
            $"(01) {data.Gtin}",
            $"(10) {data.Lot}",
            $"(17) {data.Expiry:yyMMdd}"
        };

        if (data.Manufacture is { } mfg)
        {
            lines.Add($"(11) {mfg:yyMMdd}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static Bitmap CreateBarcodeBitmap(string payload)
    {
        var encodingOptions = new EncodingOptions
        {
            Width = 200,
            Height = 200,
            Margin = 0
        };

        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.DATA_MATRIX,
            Options = encodingOptions,
            Renderer = new BitmapRenderer()
        };

        return writer.Write(payload);
    }
}
