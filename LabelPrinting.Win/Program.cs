using System.Windows.Forms;

namespace LabelPrinting.Win;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new PrintShellForm());
    }
}
