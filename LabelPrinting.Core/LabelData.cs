namespace LabelPrinting.Core;

/// <summary>
/// Immutable semantic representation of one GS1 label payload.
/// </summary>
public sealed class LabelData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LabelData"/> class.
    /// </summary>
    /// <param name="gtin">The Global Trade Item Number (GTIN) for the product.</param>
    /// <param name="lot">The lot or batch number.</param>
    /// <param name="expiry">The expiry date of the product.</param>
    /// <param name="manufacture">The optional manufacture date of the product. Defaults to null if not provided.</param>
    /// <param name="isExpiryDayZero">Indicates if the expiry date was originally specified with day "00" (meaning last day of month).</param>
    /// <exception cref="ArgumentException">Thrown when gtin or lot is null, empty, or whitespace.</exception>
    public LabelData(string gtin, string lot, DateTime expiry, DateTime? manufacture = null, bool isExpiryDayZero = false)
    {
        if (string.IsNullOrWhiteSpace(gtin)) throw new ArgumentException("GTIN is required", nameof(gtin));
        if (string.IsNullOrWhiteSpace(lot)) throw new ArgumentException("Lot is required", nameof(lot));

        Gtin = gtin.Trim();
        Lot = lot.Trim();
        Expiry = expiry.Date;
        Manufacture = manufacture?.Date;
        IsExpiryDayZero = isExpiryDayZero;
    }

    public string Gtin { get; }

    public string Lot { get; }

    public DateTime Expiry { get; }

    public DateTime? Manufacture { get; }

    /// <summary>
    /// Gets a value indicating whether the expiry date was originally specified with day "00",
    /// meaning the last day of the month. When true, human-readable output should display "00"
    /// while the barcode should use the actual last day of the month.
    /// </summary>
    public bool IsExpiryDayZero { get; }
}
