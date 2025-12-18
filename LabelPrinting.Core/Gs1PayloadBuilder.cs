using System.Globalization;
using System.Text;

namespace LabelPrinting.Core;

/// <summary>
/// Builds GS1 AI payload strings without binding to any rendering concerns.
/// </summary>
public static class Gs1PayloadBuilder
{
    /// <summary>
    /// Compose the GS1 string for a DataMatrix symbol using standard AIs.
    /// </summary>
    /// <param name="data">The label data containing GTIN, lot, expiry, and optional manufacture date.</param>
    /// <returns>A GS1 formatted string with AI (01) for GTIN, (17) for expiry, (11) for manufacture (if present), and (10) for lot number.</returns>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    public static string Build(LabelData data)
    {
        if (data is null) throw new ArgumentNullException(nameof(data));

        var builder = new StringBuilder();
        builder.Append("(01)");
        builder.Append(PadGtin(data.Gtin));
        builder.Append("(17)");
        builder.Append(FormatDate(data.Expiry));
        if (data.Manufacture is { } mfg)
        {
            builder.Append("(11)");
            builder.Append(FormatDate(mfg));
        }

        builder.Append("(10)");
        builder.Append(data.Lot);
        return builder.ToString();
    }

    /// <summary>
    /// Pads the GTIN to 14 digits by left-padding with zeros.
    /// </summary>
    /// <param name="gtin">The GTIN string to pad.</param>
    /// <returns>A 14-character GTIN string, left-padded with zeros if necessary.</returns>
    private static string PadGtin(string gtin)
    {
        const int length = 14;
        var digitsOnly = gtin.Trim();
        return digitsOnly.PadLeft(length, '0');
    }

    /// <summary>
    /// Formats a date to GS1 standard format (yyMMdd).
    /// </summary>
    /// <param name="value">The date value to format.</param>
    /// <returns>A string in yyMMdd format using invariant culture.</returns>
    private static string FormatDate(DateTime value) => value.ToString("yyMMdd", CultureInfo.InvariantCulture);
}
