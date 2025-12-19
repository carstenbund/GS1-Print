# Zero-Day Expiration Date Implementation

## Overview
This implementation handles expiration dates with day "00" (e.g., `2025-02-00`) which conventionally means "last day of the month".

## Changes Made

### 1. LabelData.cs
- Added `IsExpiryDayZero` property to track when an expiry date was originally specified with day "00"
- This flag enables different formatting for human-readable vs barcode output

### 2. CsvLabelSource.cs
- Modified `TryParseDate` method to:
  - Detect dates in format `yyyy-MM-00`
  - Calculate the actual last day of the month using `DateTime.DaysInMonth(year, month)`
  - Set `isExpiryDayZero = true` flag
  - Return the DateTime with the actual last day (28/29/30/31)

### 3. Gs1PayloadBuilder.cs
- Updated `FormatHumanExpiry` to:
  - Check `isExpiryDayZero` flag
  - If true, append "-00" to the human-readable format (e.g., "2025-02-00")
  - If false, use standard format (e.g., "2025-02")
- Updated `Gs1HumanReadableBuilder.Build` to pass the flag to `FormatHumanExpiry`

### 4. GdiLabelRenderer.cs
- Updated `BuildHumanReadable` to display "-00" in the bold "EXP" line when appropriate

## Behavior Examples

### Test Case 1: February 2025 (Non-leap year)
- **Input**: `2025-02-00`
- **Stored DateTime**: `2025-02-28` (last day calculated)
- **Barcode (AI 17)**: `250228` (yyMMdd format = February 28, 2025)
- **Human-readable (AI 17)**: `2025-02-00`
- **Bold EXP line**: `EXP  2025-02-00`

### Test Case 2: April 2025 (30 days)
- **Input**: `2025-04-00`
- **Stored DateTime**: `2025-04-30`
- **Barcode (AI 17)**: `250430` (April 30, 2025)
- **Human-readable (AI 17)**: `2025-04-00`
- **Bold EXP line**: `EXP  2025-04-00`

### Test Case 3: June 2025 (30 days)
- **Input**: `2025-06-00`
- **Stored DateTime**: `2025-06-30`
- **Barcode (AI 17)**: `250630` (June 30, 2025)
- **Human-readable (AI 17)**: `2025-06-00`
- **Bold EXP line**: `EXP  2025-06-00`

### Test Case 4: February 2024 (Leap year)
- **Input**: `2024-02-00`
- **Stored DateTime**: `2024-02-29` (leap year)
- **Barcode (AI 17)**: `240229` (February 29, 2024)
- **Human-readable (AI 17)**: `2024-02-00`
- **Bold EXP line**: `EXP  2024-02-00`

### Test Case 5: December 2025 (31 days)
- **Input**: `2025-12-00`
- **Stored DateTime**: `2025-12-31`
- **Barcode (AI 17)**: `251231` (December 31, 2025)
- **Human-readable (AI 17)**: `2025-12-00`
- **Bold EXP line**: `EXP  2025-12-00`

### Test Case 6: January 2025 (31 days)
- **Input**: `2025-01-00`
- **Stored DateTime**: `2025-01-31`
- **Barcode (AI 17)**: `250131` (January 31, 2025)
- **Human-readable (AI 17)**: `2025-01-00`
- **Bold EXP line**: `EXP  2025-01-00`

## GS1 DataMatrix Compatibility

The ZXing library used for barcode generation will receive the actual last day of the month in the GS1 payload:
- For `2025-02-00` input → payload will contain `17250228` (AI 17 + February 28, 2025)
- This is a valid GS1 date format that barcode scanners can read

The human sees "2025-02-00" on the label, while the barcode scanner reads the actual expiration date "2025-02-28", which is semantically correct since "00" conventionally means "end of month".

## Testing

Use the provided `test-zero-day.csv` file to test the implementation. It contains various months with different numbers of days, including a leap year February.

Run the application with:
```
LabelPrinting.Win.exe test-zero-day.csv
```

Inspect the generated labels to verify:
1. Human-readable text shows "-00" for day
2. Barcode encodes the correct last day of the month (28, 29, 30, or 31)
