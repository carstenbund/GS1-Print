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
- **Stored DateTime**: `2025-02-28` (last day calculated - NOZERO logic)
- **Barcode (AI 17)**: `250228` (yyMMdd format = February 28, 2025)
- **Human-readable (AI 17)**: `2025-02-00` (shows "-00")
- **Bold EXP line**: `EXP  2025-02` (no "-00")

### Test Case 2: April 2025 (30 days)
- **Input**: `2025-04-00`
- **Stored DateTime**: `2025-04-30` (NOZERO logic)
- **Barcode (AI 17)**: `250430` (April 30, 2025)
- **Human-readable (AI 17)**: `2025-04-00` (shows "-00")
- **Bold EXP line**: `EXP  2025-04` (no "-00")

### Test Case 3: June 2025 (30 days)
- **Input**: `2025-06-00`
- **Stored DateTime**: `2025-06-30` (NOZERO logic)
- **Barcode (AI 17)**: `250630` (June 30, 2025)
- **Human-readable (AI 17)**: `2025-06-00` (shows "-00")
- **Bold EXP line**: `EXP  2025-06` (no "-00")

### Test Case 4: February 2024 (Leap year)
- **Input**: `2024-02-00`
- **Stored DateTime**: `2024-02-29` (leap year - NOZERO logic)
- **Barcode (AI 17)**: `240229` (February 29, 2024)
- **Human-readable (AI 17)**: `2024-02-00` (shows "-00")
- **Bold EXP line**: `EXP  2024-02` (no "-00")

### Test Case 5: December 2025 (31 days)
- **Input**: `2025-12-00`
- **Stored DateTime**: `2025-12-31` (NOZERO logic)
- **Barcode (AI 17)**: `251231` (December 31, 2025)
- **Human-readable (AI 17)**: `2025-12-00` (shows "-00")
- **Bold EXP line**: `EXP  2025-12` (no "-00")

### Test Case 6: January 2025 (31 days)
- **Input**: `2025-01-00`
- **Stored DateTime**: `2025-01-31` (NOZERO logic)
- **Barcode (AI 17)**: `250131` (January 31, 2025)
- **Human-readable (AI 17)**: `2025-01-00` (shows "-00")
- **Bold EXP line**: `EXP  2025-01` (no "-00")

## Display Format Summary

When a date with day "00" is encountered, the system produces three different outputs:

| Component | Format | Example for 2028-11-00 | Purpose |
|-----------|--------|------------------------|---------|
| **Bold EXP Line** | `yyyy-MM` | `EXP  2028-11` | Clear, clean display for humans |
| **Human-readable (AI 17)** | `yyyy-MM-00` | `(17)2028-11-00` | Shows original "00" specification |
| **GS1 Barcode (AI 17)** | `yyMMdd` | `281130` | Actual last day for scanners |

## NOZERO Logic (Last Day Calculation)

The code calculates the actual last day of the month and stores it in the DateTime:
- February (non-leap): day 28
- February (leap year): day 29
- April, June, September, November: day 30
- January, March, May, July, August, October, December: day 31

**This logic is preserved** in case regulations change in the future. The barcode will always contain a valid, scannable date.

## GS1 DataMatrix Compatibility

The ZXing library used for barcode generation receives the actual last day of the month in the GS1 payload:
- For `2028-11-00` input → DateTime stores `2028-11-30` → payload contains `17281130` (AI 17 + November 30, 2028)
- This is a valid GS1 date format that barcode scanners can read

The barcode scanner reads the actual expiration date (e.g., "2028-11-30"), which is semantically correct since "00" conventionally means "end of month".

## Testing

Use the provided `test-zero-day.csv` file to test the implementation. It contains various months with different numbers of days, including a leap year February.

Run the application with:
```
LabelPrinting.Win.exe test-zero-day.csv
```

Inspect the generated labels to verify:
1. Human-readable text shows "-00" for day
2. Barcode encodes the correct last day of the month (28, 29, 30, or 31)
