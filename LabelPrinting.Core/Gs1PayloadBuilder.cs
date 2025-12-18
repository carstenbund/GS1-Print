using System;
using System.Globalization;
using System.Text;

namespace LabelPrinting.Core
{
    // =========================
    // Data model
    // =========================

    public sealed class LabelData
    {
        public string Gtin { get; init; } = string.Empty;
        public DateTime Expiry { get; init; }
        public DateTime? Manufacture { get; init; }
        public string Lot { get; init; } = string.Empty;
    }

    // =========================
    // Internal GS1 formatting helpers
    // =========================

    internal static class Gs1Formatting
    {
        internal static string PadGtin(string gtin)
        {
            const int length = 14;
            return gtin.Trim().PadLeft(length, '0');
        }

        /// <summary>
        /// GS1 machine date format (mandatory for DataMatrix payloads)
        /// </summary>
        internal static string FormatGs1Date(DateTime value) =>
            value.ToString("yyMMdd", CultureInfo.InvariantCulture);

        /// <summary>
        /// Human-readable expiry date format for labels / UI
        /// </summary>
        internal static string FormatHumanExpiry(DateTime value) =>
            value.ToString("yyyy-MM", CultureInfo.InvariantCulture);
    }

    // =========================
    // Human-readable GS1 builder
    // =========================

    public static class Gs1HumanReadableBuilder
    {
        public static string Build(LabelData data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            var sb = new StringBuilder();

            sb.Append("(01)");
            sb.Append(Gs1Formatting.PadGtin(data.Gtin));

            sb.Append("(17)");
            sb.Append(Gs1Formatting.FormatHumanExpiry(data.Expiry));

            if (data.Manufacture is { } mfg)
            {
                sb.Append("(11)");
                sb.Append(Gs1Formatting.FormatGs1Date(mfg));
            }

            sb.Append("(10)");
            sb.Append(data.Lot);

            return sb.ToString();
        }
    }

    // =========================
    // ZXing / DataMatrix payload builder
    // =========================

    public static class Gs1ZxingPayloadBuilder
    {
        private const char GS = '\x1D'; // ASCII 29 (Group Separator / FNC1)

        public static string Build(LabelData data)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));

            var sb = new StringBuilder();

            // Explicit GS for ZXing compatibility (as proven in your environment)
            sb.Append(GS);

            sb.Append("01").Append(Gs1Formatting.PadGtin(data.Gtin));
            sb.Append("17").Append(Gs1Formatting.FormatGs1Date(data.Expiry));
            sb.Append("10").Append(data.Lot);

            // Terminate variable-length AI (10) if another AI follows
            if (data.Manufacture is { } mfg)
            {
                sb.Append(GS);
                sb.Append("11").Append(Gs1Formatting.FormatGs1Date(mfg));
            }

            return sb.ToString();
        }
    }
}
