# ImageContrastApp

Windows Forms приложение на C# для:
- загрузки изображения;
- изменения контраста (brightness-contrast correction);
- сохранения результата.

## Функционал
- `Load`: выбор файла через `OpenFileDialog` (`jpg/jpeg/png/bmp`).
- `Apply Contrast`: применение коэффициента контраста к каждому пикселю.
- `Save`: сохранение через `SaveFileDialog` в `png/jpg/bmp`.
- Обработка пикселей через `LockBits` + `Marshal.Copy` (быстрее, чем `GetPixel/SetPixel`).

## Запуск (Visual Studio)
1. Откройте `WinForm project.sln` или `ImageContrastApp/ImageContrastApp.csproj`.
2. Убедитесь, что выбран профиль `Debug | Any CPU`.
3. Запустите `F5`.

## Запуск (CLI)
```powershell
dotnet build .\ImageContrastApp\ImageContrastApp.csproj
dotnet run --project .\ImageContrastApp\ImageContrastApp.csproj
```

## Требования
- .NET SDK `10.0+`
- Runtime `Microsoft.WindowsDesktop.App` `10.0+`
- (Опционально) Visual Studio с workload `.NET Desktop Development`

## Структура
- `ImageContrastApp/Program.cs` - точка входа.
- `ImageContrastApp/MainForm.cs` - UI и логика загрузки/обработки/сохранения.
- `.vscode/launch.json`, `.vscode/tasks.json` - запуск и сборка из VS Code.
