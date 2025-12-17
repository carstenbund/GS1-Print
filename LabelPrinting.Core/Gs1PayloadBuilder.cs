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

    private static string PadGtin(string gtin)
    {
        const int length = 14;
        var digitsOnly = gtin.Trim();
        return digitsOnly.PadLeft(length, '0');
    }

    private static string FormatDate(DateTime value) => value.ToString("yyMMdd", CultureInfo.InvariantCulture);
}
