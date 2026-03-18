# ImageContrastApp

Windows Forms application for:
- loading an image;
- applying average-based global contrast correction;
- saving the processed result.

## What stays on `main`
- Modernized WinForms styling from the feature work: centered image card, rounded buttons, light/dark theme, cleaner top toolbar.
- Split file structure for maintainability.
- One algorithm only: global contrast change relative to the average luminance of the whole image.

## What is intentionally removed from `main`
- `Local TV` mode.
- Local window parameters, presets, and fragment-based controls.
- Any previous midpoint-based contrast logic around `0.5`.

`Local TV` will be rewritten later from scientific sources as a separate, cleaner implementation.

## Global Contrast Algorithm
Formula:
```text
z = y + k * (y - y_avg)
```

Where:
- `y` is the luminance of the current pixel.
- `z` is the new luminance after contrast correction.
- `y_avg` is the average luminance of the full image.
- `k` is the user-controlled global contrast coefficient.

Interpretation:
- `k = 0` keeps the image unchanged.
- `k > 0` increases contrast.
- `-1 < k < 0` decreases contrast.

Implementation notes:
- Pixel processing uses `LockBits` + `Marshal.Copy` for performance.
- Contrast is applied in the luminance domain.
- RGB color is reconstructed via luminance ratio scaling to preserve hue better than per-channel stretching.

## UI Features
- `Load`, `Apply Contrast`, `Save` action buttons.
- Light and dark theme selector.
- Image viewport centered with padding and roughly 90% fill of the available canvas.
- Rounded controls and modernized neutral palette.

## Run (Visual Studio)
1. Open `WinForm project.sln` or `ImageContrastApp/ImageContrastApp.csproj`.
2. Restore packages if prompted.
3. Start with `F5`.

## Run (CLI)
```powershell
dotnet build .\ImageContrastApp\ImageContrastApp.csproj
dotnet run --project .\ImageContrastApp\ImageContrastApp.csproj
```

## Requirements
- .NET SDK `10.0+`
- Runtime `Microsoft.WindowsDesktop.App` `10.0+`
- Optional: Visual Studio with `.NET Desktop Development`

## Structure
- `ImageContrastApp/Program.cs` - entry point.
- `ImageContrastApp/MainForm.cs` - form layout and control initialization.
- `ImageContrastApp/MainForm.Actions.cs` - form actions, image loading, saving, disposal.
- `ImageContrastApp/MainForm.Styling.cs` - theme, rounded controls, and layout styling helpers.
- `ImageContrastApp/ImageContrastProcessor.cs` - average-based global contrast algorithm.
- `.vscode/launch.json`, `.vscode/tasks.json` - VS Code run/build config.
