using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using LabelPrinting.Core;

namespace LabelPrinting.Win;

public static class DefaultLayouts
{
    private const string ConfigFileName = "config.xml";
    private const float DefaultMarginMm = 1f;

    private static readonly LabelLayout DefaultGs1DataMatrixLayout = new(
        widthMm: 25f,
        heightMm: 20f,
        barcodeRectMm: new RectangleF(5.5f, 1.5f, 8f, 8f),
        textRectMm: new RectangleF(1.5f, 10f, 22f, 9f));

    /// <summary>
    /// Baseline 25x20mm label with a centered DataMatrix and text underneath.
    /// Values can be overridden via <see cref="ConfigFileName"/> in the app folder.
    /// </summary>
    public static LabelLayout Gs1DataMatrixLabel { get; } = LoadGs1DataMatrixLabel();

    /// <summary>
    /// Margin between labels, read from <see cref="ConfigFileName"/> so spacing can be tuned without recompiling.
    /// </summary>
    public static float LabelMarginMm { get; } = LoadMargin();

    private static LabelLayout LoadGs1DataMatrixLabel()
    {
        var path = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        if (!File.Exists(path)) return DefaultGs1DataMatrixLayout;

        try
        {
            var doc = XDocument.Load(path);
            var layoutElement = doc.Root?.Element("Gs1DataMatrixLabel");
            if (layoutElement is null) return DefaultGs1DataMatrixLayout;

            var width = ParseElementFloat(layoutElement, "WidthMm", DefaultGs1DataMatrixLayout.WidthMm);
            var height = ParseElementFloat(layoutElement, "HeightMm", DefaultGs1DataMatrixLayout.HeightMm);
            var barcodeRect = ParseRect(layoutElement.Element("BarcodeRectMm"), DefaultGs1DataMatrixLayout.BarcodeRectMm);
            var textRect = ParseRect(layoutElement.Element("TextRectMm"), DefaultGs1DataMatrixLayout.TextRectMm);

            return new LabelLayout(width, height, barcodeRect, textRect);
        }
        catch
        {
            return DefaultGs1DataMatrixLayout;
        }
    }

    private static float LoadMargin()
    {
        var path = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        if (!File.Exists(path)) return DefaultMarginMm;

        try
        {
            var doc = XDocument.Load(path);
            var marginElement = doc.Root?.Element("MarginMm");
            return marginElement is not null
                && float.TryParse(marginElement.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var marginMm)
                ? Math.Max(0, marginMm)
                : DefaultMarginMm;
        }
        catch
        {
            return DefaultMarginMm;
        }
    }

    private static float ParseElementFloat(XElement parent, string name, float fallback)
    {
        var element = parent.Element(name);
        return element is not null && float.TryParse(element.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }

    private static RectangleF ParseRect(XElement? element, RectangleF fallback)
    {
        if (element is null) return fallback;

        float x = ParseAttributeFloat(element, "x", fallback.X);
        float y = ParseAttributeFloat(element, "y", fallback.Y);
        float width = ParseAttributeFloat(element, "width", fallback.Width);
        float height = ParseAttributeFloat(element, "height", fallback.Height);

        return new RectangleF(x, y, width, height);
    }

    private static float ParseAttributeFloat(XElement element, string name, float fallback)
    {
        var attribute = element.Attribute(name);
        return attribute is not null && float.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;
    }
}
