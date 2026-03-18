# AGENTS.md

Repository context for future Codex sessions.

## Project Goal
- Small WinForms application for loading an image, applying contrast correction, and saving the result.

## Current Stack
- C#
- Windows Forms
- `TargetFramework`: `net10.0-windows`
- Single project: `ImageContrastApp`

## Current Main Branch Policy
- `main` keeps only the average-based global contrast algorithm.
- Do not reintroduce the old midpoint-based linear contrast around `0.5`.
- Do not reintroduce the previous `Local TV` implementation from `feature/localtv`.
- If `Local TV` returns later, it should be rewritten cleanly from scientific articles, not copied from the old branch.

## Where Things Live
- `ImageContrastApp/Program.cs`: app entry point.
- `ImageContrastApp/MainForm.cs`: layout and form control initialization.
- `ImageContrastApp/MainForm.Actions.cs`: form event handlers, file dialogs, bitmap lifecycle.
- `ImageContrastApp/MainForm.Styling.cs`: theme logic, rounded controls, image viewport layout.
- `ImageContrastApp/ImageContrastProcessor.cs`: average-based global contrast processor.
- `.vscode/launch.json`: VS Code debug config.
- `.vscode/tasks.json`: VS Code build task.

## Working Commands
- Build: `dotnet build .\ImageContrastApp\ImageContrastApp.csproj`
- Run: `dotnet run --project .\ImageContrastApp\ImageContrastApp.csproj`

## Implementation Rules
- Keep the split structure: UI layout, UI actions, UI styling, algorithm processor.
- Keep image processing on `LockBits`/`Marshal.Copy`.
- Dispose `Bitmap` instances carefully to avoid file locks and leaks.
- Keep the UI compact; avoid adding extra controls to `main` unless they directly support the global algorithm.
- Preserve the modernized styling already merged from the feature work.

## Before Commit
- `dotnet build` passes.
- No build artifacts in git (`bin/`, `obj/`).
- `README.md` matches the real app behavior.
