# LabelPrinting

Deterministic GS1 DataMatrix label renderer with fixed physical dimensions and minimal Windows UI. The solution is split into a UI-neutral core and a thin WinForms host for printer orchestration.

## Solution layout

- `LabelPrinting.Core` – domain-only library with GS1 payload building, millimeter-based label geometry, and CSV input parsing.
- `LabelPrinting.Win` – Windows Forms bootstrap that asks for a CSV, lets the operator pick a printer, and streams labels to `PrintDocument` using a pure millimeter layout and GDI+ renderer.

## Execution flow

1. `Program` launches `PrintShellForm`.
2. The form prompts for a CSV or Excel file, reads it via `CsvLabelSource`, and opens the printer dialog.
3. `PrintJob` paginates in millimeters with `PageComposer` and delegates each slot to `GdiLabelRenderer`.
4. `GdiLabelRenderer` builds two GS1 strings per label: `Gs1ZxingPayloadBuilder` feeds the DataMatrix encoder, while `Gs1HumanReadableBuilder` supplies the on-label text drawn using coordinates from `LabelLayout`.
5. Printing is offset to the printer's reported margins so label geometry is composed within the actual printable area and avoids clipping.

## Core concepts

- **LabelData** – immutable semantic payload (GTIN, lot, expiry, optional manufacture date).
- **LabelLayout** – single source of truth for physical dimensions expressed in millimeters.
- **Gs1ZxingPayloadBuilder / Gs1HumanReadableBuilder** – AI-aware string composers; no rendering concerns.
- **CsvLabelSource** – header-driven CSV reader (Gtin, Lot, Expiry, Manufacture columns; dates as `yyyy-MM-dd`).

## Default layout

`DefaultLayouts.Gs1DataMatrixLabel` describes a 25×20 mm label with a centered 14×14 mm DataMatrix and a text block beneath it. Adjust the geometry without recompiling by editing `LabelPrinting.Win/config.xml` (values are in millimeters and documented inline); the application falls back to the built-in defaults if the file is missing or invalid. The same file exposes `PageInsetMm` to keep ink away from the paper edge and `LabelMarginMm` for tuning the spacing between labels—set the latter to zero to pack labels with no gap.

## Dependencies

- .NET 8 Windows Forms
- ZXing.Net for GS1 DataMatrix generation
- System.Drawing.Common for GDI+ rendering
- ExcelDataReader for Excel ingestion (xlsx/xls)
