using System.Drawing;
using System.Drawing.Printing;
using LabelPrinting.Core;

namespace LabelPrinting.Win;

/// <summary>
/// Coordinates page events and delegates drawing to the renderer.
/// </summary>
public sealed class PrintJob
{
    private readonly LabelLayout _layout;
    private readonly PageComposer _composer;
    private readonly GdiLabelRenderer _renderer;
    private Queue<LabelData>? _pending;

    public PrintJob(LabelLayout layout, PageComposer composer, GdiLabelRenderer renderer)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _composer = composer ?? throw new ArgumentNullException(nameof(composer));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public void Print(IEnumerable<LabelData> labels, PrinterSettings printerSettings)
    {
        if (labels is null) throw new ArgumentNullException(nameof(labels));
        if (printerSettings is null) throw new ArgumentNullException(nameof(printerSettings));

        _pending = new Queue<LabelData>(labels);
        using var document = new PrintDocument
        {
            PrinterSettings = printerSettings
        };

        document.PrintPage += OnPrintPage;
        document.Print();
    }

    private void OnPrintPage(object? sender, PrintPageEventArgs e)
    {
        if (_pending is null)
        {
            e.HasMorePages = false;
            return;
        }

        var graphics = e.Graphics;
        graphics.PageUnit = GraphicsUnit.Millimeter;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

        var pageRectMm = new RectangleF(
            0,
            0,
            HundredthsInchToMm(e.PageBounds.Width),
            HundredthsInchToMm(e.PageBounds.Height));

        var slots = _composer.GetLabelSlots(pageRectMm, _layout);
        foreach (var slot in slots)
        {
            if (_pending.Count == 0)
            {
                break;
            }

            var labelData = _pending.Dequeue();
            _renderer.DrawLabel(graphics, slot, _layout, labelData);
        }

        e.HasMorePages = _pending.Count > 0;
    }

    private static float HundredthsInchToMm(float hundredths) => hundredths / 100f * 25.4f;
}
