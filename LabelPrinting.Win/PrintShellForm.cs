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
    private readonly PageComposer _composer = new(DefaultLayouts.LabelMarginMm); // margin now configurable via config.xml
    private readonly LabelLayout _layout = DefaultLayouts.Gs1DataMatrixLabel;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrintShellForm"/> class.
    /// </summary>
    public PrintShellForm()
    {
        Text = "Label Printing";
        Width = 400;
        Height = 200;
        Shown += OnShown;
    }

    /// <summary>
    /// Handles the form's Shown event and initiates the printing workflow.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnShown(object? sender, EventArgs e)
    {
        BeginInvoke(new MethodInvoker(RunWorkflow));
    }

    /// <summary>
    /// Executes the complete printing workflow: load labels, select printer, and print.
    /// The form closes automatically when workflow completes or if user cancels.
    /// </summary>
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

    /// <summary>
    /// Displays a file dialog for the user to select a CSV or Excel file and loads label data from it.
    /// </summary>
    /// <returns>A list of <see cref="LabelData"/> objects loaded from the selected CSV file, or an empty list if user cancels.</returns>
    private List<LabelData> LoadLabels()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Label data (*.csv;*.xlsx;*.xls)|*.csv;*.xlsx;*.xls|CSV files (*.csv)|*.csv|Excel files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*",
            Title = "Select label batch file"
        };

        return dialog.ShowDialog(this) == DialogResult.OK
            ? new CsvLabelSource(dialog.FileName).Read().ToList()
            : new List<LabelData>();
    }

    /// <summary>
    /// Displays a printer selection dialog for the user to choose a printer.
    /// </summary>
    /// <returns>The selected <see cref="PrinterSettings"/>, or null if the user cancels the dialog.</returns>
    private PrinterSettings? SelectPrinter()
    {
        using var dialog = new PrintDialog { UseEXDialog = true };
        return dialog.ShowDialog(this) == DialogResult.OK
            ? dialog.PrinterSettings
            : null;
    }
}
