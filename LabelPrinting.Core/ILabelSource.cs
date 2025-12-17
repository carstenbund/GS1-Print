namespace LabelPrinting.Core;

public interface ILabelSource
{
    IEnumerable<LabelData> Read();
}
