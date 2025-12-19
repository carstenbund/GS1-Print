using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
    private static readonly string[] SupportedDateFormats =
    {
        "yyyy-MM-dd",
        "yyyy-M-d",
        "yyyy/MM/dd",
        "yyyy/M/d",
        "yyyyMMdd",
        "yyyy-MM",
        "yyyyMM",
        "MM/dd/yyyy",
        "M/d/yyyy",
        "dd/MM/yyyy",
        "d/M/yyyy",
    };

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

        var delimiter = DetectDelimiter(header);
        var columns = ParseDelimitedLine(header, delimiter)
            .Select(h => h.Trim().ToLowerInvariant())
            .ToArray();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = ParseDelimitedLine(line, delimiter)
                .Select(c => c.Trim())
                .ToArray();
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

    private static char DetectDelimiter(string header)
    {
        var delimiters = new[] { ',', ';', '\t', '|' };
        var best = delimiters
            .Select(d => new { Delimiter = d, Count = header.Count(c => c == d) })
            .OrderByDescending(x => x.Count)
            .FirstOrDefault(x => x.Count > 0);

        return best?.Delimiter ?? ',';
    }

    private static string[] ParseDelimitedLine(string line, char delimiter)
    {
        var values = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == delimiter && !inQuotes)
            {
                values.Add(builder.ToString().Trim());
                builder.Clear();
            }
            else
            {
                builder.Append(c);
            }
        }

        values.Add(builder.ToString().Trim());
        return values.ToArray();
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
        if (!TryGetValue(dict, out var gtin, "gtin")) return false;
        if (!TryGetValue(dict, out var lot, "lot")) return false;
        if (!TryGetValue(dict, out var expiryRaw, "expiry", "exp")) return false;

        if (!TryParseDate(expiryRaw, out var expiry))
        {
            return false;
        }

        DateTime? manufacture = null;
        if (TryGetValue(dict, out var mfgRaw, "manufacture", "mfg") &&
            TryParseDate(mfgRaw, out var mfgDate))
        {
            manufacture = mfgDate;
        }

        label = new LabelData(gtin, lot, expiry, manufacture);
        return true;
    }

    private static bool TryParseDate(string value, out DateTime date)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            date = default;
            return false;
        }

        normalized = StripTimeComponent(normalized);

        var zeroDayMatch = Regex.Match(normalized, @"^(?<year>\d{4})-(?<month>\d{2})-00$");
        if (zeroDayMatch.Success
            && int.TryParse(zeroDayMatch.Groups["year"].Value, out var year)
            && int.TryParse(zeroDayMatch.Groups["month"].Value, out var month)
            && month is >= 1 and <= 12)
        {
            date = new DateTime(year, month, 1);
            return true;
        }

        if (DateTime.TryParseExact(normalized, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out date))
        {
            return true;
        }

        if (double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
        {
            try
            {
                date = DateTime.FromOADate(oaDate);
                return true;
            }
            catch (ArgumentException)
            {
                // fall through to general parsing
            }
        }

        return DateTime.TryParse(normalized, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out date);
    }

    private static string StripTimeComponent(string value)
    {
        var delimiters = new[] { ' ', 'T' };
        foreach (var delimiter in delimiters)
        {
            var index = value.IndexOf(delimiter);
            if (index <= 0 || index == value.Length - 1) continue;

            var potentialTime = value[(index + 1)..].Trim();
            if (TimeSpan.TryParse(potentialTime, out _))
            {
                return value[..index];
            }
        }

        return value;
    }

    private static bool IsExcelExtension(string extension)
    {
        return string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase)
               || string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryGetValue(Dictionary<string, string> dict, out string value, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (dict.TryGetValue(key, out value!))
            {
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static void RegisterCodePagesIfNeeded()
    {
        if (_codePagesRegistered) return;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _codePagesRegistered = true;
    }
}
