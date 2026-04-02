# ImageContrastApp

Windows Forms application for:
- loading an image;
- applying grayscale contrast enhancement;
- saving the processed result.

## Current Branch
This `smooth_sigma` branch contains the grayscale version with:
- automatic target standard deviation `σz`;
- automatic `q` for local method 3.

Implemented modes:
- global television-style contrast transform for grayscale brightness;
- local fragment contrast transform for grayscale brightness with methods 1, 2, and 3.

Color images can still be loaded, but they are converted to grayscale brightness before processing. Output images are saved as grayscale (`R = G = B`).

## Processing Model
All processing in this branch works on grayscale brightness:

```text
y = 0.2126R + 0.7152G + 0.0722B
```

Brightness values are rounded to the nearest integer and clamped to `[0, 255]`.

## Adaptive Target Standard Deviation
This branch no longer uses manual `σz`.

Instead, the target standard deviation is chosen automatically from the source-image standard deviation:

```text
sigma_z = sigma_y + 0.5 * (80 - sigma_y)
```

Equivalent form:

```text
sigma_z = 0.5 * sigma_y + 40
```

Interpretation:
- if the source image has low contrast, `sigma_z` is pushed upward;
- if the source image already has higher contrast, `sigma_z` stays closer to the source;
- this makes enhancement strength image-dependent rather than manually fixed.

## Global TV Contrast
The global mode applies a television-style grayscale transform:

```text
z = y + k(y - y_bar)
k = sigma_z / sigma_y - 1
```

Where:
- `y` is the source brightness;
- `z` is the transformed brightness;
- `y_bar` is the global mean brightness;
- `sigma_y` is the source-image standard deviation;
- `sigma_z` is the adaptive target standard deviation.

Notes:
- if `sigma_y = 0`, the image remains unchanged;
- the output is grayscale only.

## Local Fragment Methods
The local mode scans a rectangular fragment over the image with:
- horizontal stride `1` pixel;
- vertical stride `1` pixel;
- cropped fragment bounds at image borders.

Overlap handling follows the paper-style sequential rule:
- if an output pixel has not been written yet, write the new value directly;
- otherwise overwrite it with `(old + new) / 2`.

Supported local methods:

### Method 1
Uses a fragment transform driven by the adaptive global target deviation.

### Method 2
Uses a fragment-specific coefficient:

```text
k_frag = sigma_gl_z / sigma_frag_y - 1
```

If `sigma_frag_y = 0`, the fragment is copied unchanged.

### Method 3
Uses the generalized coefficient:

```text
k_frag = (sigma_gl_z / sigma_frag_y) * (sigma_frag_y / sigma_gl_y)^(1 - q) - 1
```

This branch also uses adaptive `q`:

```text
q = clamp(1 - sigma_gl_y / 80, 0, 1)
```

So this branch combines both modifications:
- adaptive `σz`;
- adaptive `q` for method 3.

## UI Features
- fixed dark UI style;
- `Load`, `Apply`, `Save` buttons;
- mode selector: `Global Contrast` / `Local Fragment`;
- local method selector: `Method 1`, `Method 2`, `Method 3`;
- fragment width and height controls;
- optional multithreaded fragment processing;
- automatic `σz` rule shown in the UI;
- adaptive `q` hint displayed for method 3;
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
- `ImageContrastApp/ImageContrastProcessor.cs` - global grayscale TV contrast with adaptive `σz`.
- `ImageContrastApp/LocalFragmentProcessing.cs` - local fragment engine, formulas, adaptive `σz`, and adaptive `q`.
- `ImageContrastApp/GrayImageBuffer.cs` - grayscale image conversion and bitmap output.
- `ImageContrastApp/BitmapPixelBuffer.cs` - low-level pixel buffer helper.
