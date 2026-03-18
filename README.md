# ImageContrastApp

Windows Forms приложение на C# для:
- загрузки изображения;
- изменения контраста (brightness-contrast correction);
- сохранения результата.

## Функционал
- `Load`: выбор файла через `OpenFileDialog` (`jpg/jpeg/png/bmp`).
- `Apply Contrast`: глобальное изменение контраста относительно средней яркости всего изображения.
- `Save`: сохранение через `SaveFileDialog` в `png/jpg/bmp`.
- Обработка пикселей через `LockBits` + `Marshal.Copy` (быстрее, чем `GetPixel/SetPixel`).

## Global Contrast Algorithm
- Formula: `z = y + k * (y - y_avg)`.
- `y_avg` is the average luminance of the whole image.
- `k = 0` keeps the image unchanged.
- `k > 0` increases contrast.
- `-1 < k < 0` decreases contrast.

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
- `ImageContrastApp/MainForm.cs` - layout и инициализация UI.
- `ImageContrastApp/MainForm.Actions.cs` - обработчики формы, загрузка и сохранение.
- `ImageContrastApp/ImageContrastProcessor.cs` - глобальный алгоритм изменения контраста по средней яркости изображения.
- `.vscode/launch.json`, `.vscode/tasks.json` - запуск и сборка из VS Code.
