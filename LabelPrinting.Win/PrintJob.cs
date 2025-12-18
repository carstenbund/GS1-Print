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

    /// <summary>
    /// Initializes a new instance of the <see cref="PrintJob"/> class.
    /// </summary>
    /// <param name="layout">The label layout to use for positioning elements on each label.</param>
    /// <param name="composer">The page composer that calculates label positions on the page.</param>
    /// <param name="renderer">The renderer that draws individual labels.</param>
    /// <exception cref="ArgumentNullException">Thrown when layout, composer, or renderer is null.</exception>
    public PrintJob(LabelLayout layout, PageComposer composer, GdiLabelRenderer renderer)
    {
        _layout = layout ?? throw new ArgumentNullException(nameof(layout));
        _composer = composer ?? throw new ArgumentNullException(nameof(composer));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    /// <summary>
    /// Sends the collection of labels to the printer, flowing across multiple pages as needed.
    /// </summary>
    /// <param name="labels">The enumerable collection of label data to print.</param>
    /// <param name="printerSettings">The printer settings specifying which printer to use and print options.</param>
    /// <exception cref="ArgumentNullException">Thrown when labels or printerSettings is null.</exception>
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

    /// <summary>
    /// Handles the PrintPage event, rendering as many labels as fit on the current page.
    /// </summary>
    /// <param name="sender">The event sender (PrintDocument).</param>
    /// <param name="e">Event arguments providing the graphics surface and page bounds.</param>
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

        var insetMm = DefaultLayouts.PageInsetMm;
        var pageWidthMm = HundredthsInchToMm(e.PageBounds.Width);
        var pageHeightMm = HundredthsInchToMm(e.PageBounds.Height);

        graphics.TranslateTransform(insetMm, insetMm);

        var pageRectMm = new RectangleF(
            0,
            0,
            pageWidthMm - insetMm * 2,
            pageHeightMm - insetMm * 2);

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

    /// <summary>
    /// Converts a measurement from hundredths of an inch to millimeters.
    /// </summary>
    /// <param name="hundredths">The value in hundredths of an inch (GDI+ default page unit).</param>
    /// <returns>The equivalent value in millimeters.</returns>
    private static float HundredthsInchToMm(float hundredths) => hundredths / 100f * 25.4f;
}
