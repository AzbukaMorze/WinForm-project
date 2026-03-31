# AGENTS.md

Repository guidance for coding agents working in this project.

## Scope
- Applies to the entire repository.
- Check the current Git branch before making branch-specific assumptions.

## Project Overview
- Desktop image-processing application built with C# and Windows Forms.
- Main project: `ImageContrastApp`
- Target framework: `net10.0-windows`

## Current Repository Layout
- `ImageContrastApp/Program.cs`: application entry point.
- `ImageContrastApp/MainForm.cs`: UI layout and control initialization.
- `ImageContrastApp/MainForm.Actions.cs`: event handlers, mode selection, image loading/saving, bitmap lifecycle.
- `ImageContrastApp/MainForm.Styling.cs`: visual styling and viewport layout helpers.
- `ImageContrastApp/ImageContrastProcessor.cs`: global grayscale TV-style contrast transform.
- `ImageContrastApp/LocalFragmentProcessing.cs`: local fragment processing engine and formulas.
- `ImageContrastApp/GrayImageBuffer.cs`: grayscale conversion/output helper.
- `ImageContrastApp/BitmapPixelBuffer.cs`: low-level pixel buffer helper.
- `.vscode/launch.json`, `.vscode/tasks.json`: local VS Code run/build configuration.

## Branch Awareness
- `main`: keep only the simpler average-based global contrast implementation unless the user explicitly requests otherwise.
- `grayscale`: grayscale-focused branch with:
  - grayscale global TV-style contrast;
  - grayscale local fragment methods 1, 2, and 3;
  - sequential overlap rule from the paper-style specification.
- If another branch is checked out, inspect the branch state before applying assumptions from `main` or `grayscale`.

## Working Commands
- Build: `dotnet build .\ImageContrastApp\ImageContrastApp.csproj`
- Run: `dotnet run --project .\ImageContrastApp\ImageContrastApp.csproj`

## Implementation Expectations
- Preserve the split structure between UI, actions, styling, and processing code.
- Prefer bitmap-buffer processing (`LockBits`, `Marshal.Copy`, array-based pixel access) over `GetPixel`/`SetPixel` in hot paths.
- Dispose `Bitmap` instances carefully to avoid leaks and file locks.
- Keep UI additions compact and directly tied to implemented processing behavior.
- Keep README and branch documentation consistent with actual behavior.

## Grayscale Branch Notes
- Processing is grayscale-first; color images may be loaded, but are converted to grayscale brightness for processing.
- Output on this branch is grayscale (`R = G = B`).
- For the local fragment method, preserve the exact sequential overlap write rule if modifying that path.
- If formulas from a paper/spec are incomplete or ambiguous, document the chosen interpretation explicitly in code comments or commit notes.

## Safety
- Do not delete user work or revert unrelated changes.
- Do not assume the running executable can be overwritten; if build fails because the app is open, explain that clearly.
- Before switching branches, check for uncommitted changes and avoid carrying branch-specific edits across branches unintentionally.

## Before Finishing
- `dotnet build` should pass when the executable is not locked by a running instance.
- No `bin/` or `obj/` artifacts should be committed.
- `README.md` should reflect the current branch behavior.
