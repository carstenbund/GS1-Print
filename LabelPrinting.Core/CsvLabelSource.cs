using System.Globalization;
using System.Text;

namespace LabelPrinting.Core;

/// <summary>
/// Reads label payloads from a simple CSV file with headers.
/// Expected columns: Gtin, Lot, Expiry (yyyy-MM-dd), Manufacture (optional).
/// </summary>
public sealed class CsvLabelSource : ILabelSource
{
    private readonly string _path;

    public CsvLabelSource(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("CSV path is required", nameof(path));
        _path = path;
    }

    public IEnumerable<LabelData> Read()
    {
        using var reader = new StreamReader(_path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var header = reader.ReadLine();
        if (header is null)
        {
            yield break;
        }

        var columns = header.Split(',').Select(h => h.Trim().ToLowerInvariant()).ToArray();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cells = line.Split(',');
            var values = cells.Select(c => c.Trim()).ToArray();
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < Math.Min(columns.Length, values.Length); i++)
            {
                dict[columns[i]] = values[i];
            }

            if (!dict.TryGetValue("gtin", out var gtin)) continue;
            if (!dict.TryGetValue("lot", out var lot)) continue;
            if (!dict.TryGetValue("expiry", out var expiryRaw)) continue;

            if (!DateTime.TryParseExact(expiryRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiry))
            {
                continue;
            }

            DateTime? manufacture = null;
            if (dict.TryGetValue("manufacture", out var mfgRaw) &&
                DateTime.TryParseExact(mfgRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mfgDate))
            {
                manufacture = mfgDate;
            }

            yield return new LabelData(gtin, lot, expiry, manufacture);
        }
    }
}
