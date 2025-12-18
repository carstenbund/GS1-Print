namespace LabelPrinting.Core;

/// <summary>
/// Defines a contract for reading label data from a source.
/// </summary>
public interface ILabelSource
{
    /// <summary>
    /// Reads label data from the source.
    /// </summary>
    /// <returns>An enumerable sequence of <see cref="LabelData"/> objects.</returns>
    IEnumerable<LabelData> Read();
}
