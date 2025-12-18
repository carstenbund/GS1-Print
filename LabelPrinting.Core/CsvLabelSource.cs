using System.Globalization;
using System.Text;
using ExcelDataReader;

namespace LabelPrinting.Core;

/// <summary>
/// Reads label payloads from a CSV or Excel file with headers.
/// Expected columns: Gtin, Lot, Expiry (yyyy-MM-dd), Manufacture (optional).
/// </summary>
public sealed class CsvLabelSource : ILabelSource
{
    private readonly string _path;
    private static bool _codePagesRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvLabelSource"/> class.
    /// </summary>
    /// <param name="path">The file system path to the CSV file containing label data.</param>
    /// <exception cref="ArgumentException">Thrown when path is null, empty, or whitespace.</exception>
    public CsvLabelSource(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("CSV path is required", nameof(path));
        _path = path;
    }

    /// <summary>
    /// Reads and parses label data from the CSV or Excel file.
    /// </summary>
    /// <returns>An enumerable sequence of <see cref="LabelData"/> objects parsed from valid CSV rows.</returns>
    /// <remarks>
    /// Expects columns: Gtin, Lot, Expiry (yyyy-MM-dd format), and optionally Manufacture (yyyy-MM-dd format).
    /// Rows with missing required fields or invalid date formats are skipped.
    /// </remarks>
    public IEnumerable<LabelData> Read()
    {
        var extension = Path.GetExtension(_path);
        if (IsExcelExtension(extension))
        {
            foreach (var label in ReadExcel())
            {
                yield return label;
            }
        }
        else
        {
            foreach (var label in ReadCsv())
            {
                yield return label;
            }
        }
    }

    private IEnumerable<LabelData> ReadCsv()
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

            if (TryCreateLabel(dict, out var label))
            {
                yield return label!;
            }
        }
    }

    private IEnumerable<LabelData> ReadExcel()
    {
        RegisterCodePagesIfNeeded();
        using var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        if (!reader.Read()) yield break;

        var columns = ReadRow(reader)
            .Select(h => h.ToLowerInvariant())
            .ToArray();

        while (reader.Read())
        {
            var values = ReadRow(reader);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < Math.Min(columns.Length, values.Length); i++)
            {
                dict[columns[i]] = values[i];
            }

            if (TryCreateLabel(dict, out var label))
            {
                yield return label!;
            }
        }
    }

    private static string[] ReadRow(IExcelDataReader reader)
    {
        var values = new string[reader.FieldCount];
        for (var i = 0; i < reader.FieldCount; i++)
        {
            values[i] = reader.GetValue(i)?.ToString()?.Trim() ?? string.Empty;
        }

        return values;
    }

    private static bool TryCreateLabel(Dictionary<string, string> dict, out LabelData? label)
    {
        label = null;
        if (!dict.TryGetValue("gtin", out var gtin)) return false;
        if (!dict.TryGetValue("lot", out var lot)) return false;
        if (!dict.TryGetValue("expiry", out var expiryRaw)) return false;

        if (!DateTime.TryParseExact(expiryRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiry))
        {
            return false;
        }

        DateTime? manufacture = null;
        if (dict.TryGetValue("manufacture", out var mfgRaw) &&
            DateTime.TryParseExact(mfgRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var mfgDate))
        {
            manufacture = mfgDate;
        }

        label = new LabelData(gtin, lot, expiry, manufacture);
        return true;
    }

    private static bool IsExcelExtension(string extension)
    {
        return string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase);
    }

    private static void RegisterCodePagesIfNeeded()
    {
        if (_codePagesRegistered) return;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _codePagesRegistered = true;
    }
}
