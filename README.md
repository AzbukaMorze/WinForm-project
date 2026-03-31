# ImageContrastApp

Windows Forms application for:
- loading an image;
- applying luminance-based contrast enhancement;
- saving the processed result.

## Current Branch
This `luminance` branch contains the color-preserving luminance version of the app.

Implemented modes:
- global contrast based on average image luminance;
- local fragment processing for color images with pluggable local processors.

Images remain color after processing. Contrast is computed from luminance, then mapped back to RGB to preserve color relationships.

## Processing Model
The branch uses luminance as the working brightness value:

```text
y = 0.2126R + 0.7152G + 0.0722B
```

The core contrast formulas operate on `y`, while the final result is reconstructed into RGB output.

## Global Contrast
The global mode applies:

```text
z = y + k(y - y_avg)
```

Where:
- `y` is the source pixel luminance;
- `z` is the target luminance;
- `y_avg` is the average luminance of the whole image;
- `k` is the user-controlled global contrast coefficient.

The luminance change is transferred back to RGB using a luminance ratio, so the image stays color instead of becoming grayscale.

## Local Fragment Mode
The local mode scans overlapping rectangular fragments across the image and applies a selected fragment processor.

Current local processor options:
- `Identity`
- `Simple Local Contrast`
- `Freq. Stretch (Later)` placeholder

This branch uses a reusable local fragment engine with:
- 1-pixel horizontal stride;
- 1-pixel vertical stride;
- cropped fragment bounds near image edges;
- overlap composition through equal-weight accumulation and normalization;
- optional multithreaded processing.

## UI Features
- fixed dark UI style;
- `Load`, `Apply`, `Save` buttons;
- mode selector: `Global Contrast` / `Local Fragment`;
- `k` control shared by global and local modes;
- local processor selector;
- fragment width and height controls;
- optional multithread toggle;
- centered image viewport with rounded controls.

## Run
```powershell
dotnet build .\ImageContrastApp\ImageContrastApp.csproj
dotnet run --project .\ImageContrastApp\ImageContrastApp.csproj
```

## Requirements
- .NET SDK `10.0+`
- Runtime `Microsoft.WindowsDesktop.App` `10.0+`

## Structure
- `ImageContrastApp/Program.cs` - entry point.
- `ImageContrastApp/MainForm.cs` - form layout and controls.
- `ImageContrastApp/MainForm.Actions.cs` - form actions and mode handling.
- `ImageContrastApp/MainForm.Styling.cs` - fixed dark styling helpers.
- `ImageContrastApp/ImageContrastProcessor.cs` - global luminance-based contrast algorithm.
- `ImageContrastApp/LocalFragmentProcessing.cs` - local fragment engine and processor plumbing.
- `ImageContrastApp/BitmapPixelBuffer.cs` - low-level pixel buffer helper.
