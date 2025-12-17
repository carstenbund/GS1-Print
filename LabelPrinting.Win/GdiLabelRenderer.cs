using System.Drawing;
using System.Drawing.Imaging;
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
    public void DrawLabel(Graphics graphics, LabelLayout layout, LabelData data)
    {
        if (graphics is null) throw new ArgumentNullException(nameof(graphics));
        if (layout is null) throw new ArgumentNullException(nameof(layout));
        if (data is null) throw new ArgumentNullException(nameof(data));

        var payload = Gs1PayloadBuilder.Build(data);
        using var barcodeBitmap = CreateBarcodeBitmap(payload, layout.BarcodeRectMm, graphics);
        var barcodePixels = MmRectToPixels(layout.BarcodeRectMm, graphics);
        graphics.DrawImage(barcodeBitmap, barcodePixels);

        var textPixels = MmRectToPixels(layout.TextRectMm, graphics);
        using var font = new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Point);
        using var format = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
        var humanReadable = BuildHumanReadable(data);
        graphics.DrawString(humanReadable, font, Brushes.Black, textPixels, format);
    }

    private static string BuildHumanReadable(LabelData data)
    {
        var lines = new List<string>
        {
            $"GTIN: {data.Gtin}",
            $"LOT:  {data.Lot}",
            $"EXP:  {data.Expiry:yyyy-MM-dd}"
        };

        if (data.Manufacture is { } mfg)
        {
            lines.Add($"MFG:  {mfg:yyyy-MM-dd}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static Bitmap CreateBarcodeBitmap(string payload, RectangleF targetMm, Graphics graphics)
    {
        var encodingOptions = new EncodingOptions
        {
            Width = (int)Math.Ceiling(MmToPixels(targetMm.Width, graphics.DpiX)),
            Height = (int)Math.Ceiling(MmToPixels(targetMm.Height, graphics.DpiY)),
            Margin = 0
        };

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.DATA_MATRIX,
            Options = encodingOptions,
            Renderer = new PixelDataRenderer()
        };

        var pixelData = writer.Write(payload);
        var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppArgb);
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return bitmap;
    }

    private static RectangleF MmRectToPixels(RectangleF rectMm, Graphics graphics) => new(
        MmToPixels(rectMm.X, graphics.DpiX),
        MmToPixels(rectMm.Y, graphics.DpiY),
        MmToPixels(rectMm.Width, graphics.DpiX),
        MmToPixels(rectMm.Height, graphics.DpiY));

    private static float MmToPixels(float mm, float dpi) => mm / 25.4f * dpi;
}
