# Import & Export

Supported formats: Maestro JSON (.maestro) and .alog (.alog).

## Supported Formats

| Format | Extension | Import | Export |
|--------|-----------|--------|--------|
| **Maestro JSON** | `.maestro` | ✅ Full support | ✅ Native format |
| **.alog** | `.alog` | ✅ Temperature + metadata | ✅ Full export |

## Import

1. Paste file content into the **Content** textarea
2. Enter the **filename** (with extension, e.g., `roast.alog`)
3. Click **Import**

The importer auto-detects the format from the file extension.

## Export

1. Select the **profile** from the dropdown
2. Choose the **format** (JSON or .alog)
3. Click **Export**

The exported content appears in the result panel for copying.

## .alog Format

The `.alog` format stores time/temperature arrays, phase events, weight/moisture/color measurements, roast UUID, and metadata — compatible with the desktop application import format.
