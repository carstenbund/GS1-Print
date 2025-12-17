namespace LabelPrinting.Core;

/// <summary>
/// Immutable semantic representation of one GS1 label payload.
/// </summary>
public sealed class LabelData
{
    public LabelData(string gtin, string lot, DateTime expiry, DateTime? manufacture = null)
    {
        if (string.IsNullOrWhiteSpace(gtin)) throw new ArgumentException("GTIN is required", nameof(gtin));
        if (string.IsNullOrWhiteSpace(lot)) throw new ArgumentException("Lot is required", nameof(lot));

        Gtin = gtin.Trim();
        Lot = lot.Trim();
        Expiry = expiry.Date;
        Manufacture = manufacture?.Date;
    }

    public string Gtin { get; }

    public string Lot { get; }

    public DateTime Expiry { get; }

    public DateTime? Manufacture { get; }
}
