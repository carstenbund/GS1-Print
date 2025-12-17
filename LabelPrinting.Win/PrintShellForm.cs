using System.Drawing.Printing;
using System.Windows.Forms;
using LabelPrinting.Core;

namespace LabelPrinting.Win;

/// <summary>
/// Minimal WinForms bootstrapper: choose CSV, choose printer, fire print job, and exit.
/// </summary>
public sealed class PrintShellForm : Form
{
    private readonly GdiLabelRenderer _renderer = new();
    private readonly PageComposer _composer = new(marginMm: 2f);
    private readonly LabelLayout _layout = DefaultLayouts.Gs1DataMatrixLabel;

    public PrintShellForm()
    {
        Text = "Label Printing";
        Width = 400;
        Height = 200;
        Shown += OnShown;
    }

    private void OnShown(object? sender, EventArgs e)
    {
        BeginInvoke(new MethodInvoker(RunWorkflow));
    }

    private void RunWorkflow()
    {
        try
        {
            var labels = LoadLabels();
            if (labels.Count == 0)
            {
                MessageBox.Show(this, "No labels found in the selected CSV.", "Empty input", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
                return;
            }

            var printerSettings = SelectPrinter();
            if (printerSettings is null)
            {
                Close();
                return;
            }

            var job = new PrintJob(_layout, _composer, _renderer);
            job.Print(labels, printerSettings);
        }
        finally
        {
            Close();
        }
    }

    private List<LabelData> LoadLabels()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Select label batch CSV"
        };

        return dialog.ShowDialog(this) == DialogResult.OK
            ? new CsvLabelSource(dialog.FileName).Read().ToList()
            : new List<LabelData>();
    }

    private PrinterSettings? SelectPrinter()
    {
        using var dialog = new PrintDialog { UseEXDialog = true };
        return dialog.ShowDialog(this) == DialogResult.OK
            ? dialog.PrinterSettings
            : null;
    }
}
