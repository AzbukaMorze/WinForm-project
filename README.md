# ImageContrastApp

Windows Forms application for:
- loading an image;
- applying grayscale contrast enhancement;
- saving the processed result.

## Current Branch
This `grayscale` branch contains a grayscale-focused version of the app.
The UI uses a simple i18n layer and switches to Russian when the current UI culture is `ru`.

Implemented modes:
- global television-style contrast transform for grayscale brightness;
- local fragment contrast transform for grayscale brightness with methods 1, 2, 3, and 4.

Color images can still be loaded, but they are converted to grayscale brightness before processing. Output images are saved as grayscale (`R = G = B`).

## Processing Model
All processing in this branch works on grayscale brightness:

```text
y = 0.2126R + 0.7152G + 0.0722B
```

Brightness values are rounded to the nearest integer and clamped to `[0, 255]`.

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
- `sigma_z` is the target standard deviation selected in the UI.

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
Uses a fragment transform driven by the global target deviation.

### Method 2
Uses a fragment-specific coefficient:

```text
k_frag = sigma_gl_z / sigma_frag_y - 1
```

If `sigma_frag_y = 0`, the fragment is copied unchanged.

### Method 3
Uses the generalized coefficient with user-controlled `q`:

```text
k_frag = (sigma_gl_z / sigma_frag_y) * (sigma_frag_y / sigma_gl_y)^(1 - q) - 1
```

In this branch, `q` is set manually in the UI.

### Method 4
Uses the same generalized coefficient as method 3, but with adaptive `q` from the previous `smoothQ` branch:

```text
q = clamp(1 - sigma_gl_y / 80, 0, 1)
```

## UI Features
- fixed dark UI style;
- localized UI text with English and Russian resources;
- mode selector for global and local processing;
- target standard deviation input `sigma_z`;
- local method selector with short Russian titles for global sigma, local sigma, manual q, and adaptive q;
- fragment width and height controls;
- manual `q` input for method 3;
- adaptive `q` hint for method 4;
- optional multithreaded fragment processing;
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
- `ImageContrastApp/ImageContrastProcessor.cs` - global grayscale TV contrast.
- `ImageContrastApp/LocalFragmentProcessing.cs` - local fragment engine and formulas.
- `ImageContrastApp/GrayImageBuffer.cs` - grayscale image conversion and bitmap output.
- `ImageContrastApp/BitmapPixelBuffer.cs` - low-level pixel buffer helper.
