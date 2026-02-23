# AGENTS.md

Контекст и инструкции для будущих сессий Codex в этом репозитории.

## Цель проекта
- Небольшое WinForms-приложение для загрузки изображения, изменения контраста и сохранения результата.

## Текущий стек
- C#
- Windows Forms
- `TargetFramework`: `net10.0-windows`
- Один проект: `ImageContrastApp`

## Где что находится
- `ImageContrastApp/Program.cs`: вход в приложение.
- `ImageContrastApp/MainForm.cs`: UI + обработчики кнопок + алгоритм контраста.
- `.vscode/launch.json`: конфиг F5 для VS Code.
- `.vscode/tasks.json`: build task для VS Code.

## Рабочие команды
- Сборка: `dotnet build .\ImageContrastApp\ImageContrastApp.csproj`
- Запуск: `dotnet run --project .\ImageContrastApp\ImageContrastApp.csproj`

## Правила доработок
- Не менять `TargetFramework`, если это не запрошено явно.
- Не добавлять тяжелые внешние библиотеки без явной причины.
- Для пиксельной обработки сохранять подход `LockBits`/`Marshal.Copy` (не откатываться к `GetPixel/SetPixel`).
- Следить за `Dispose` для `Bitmap` и `Image` во избежание утечек и блокировок файлов.
- Новые функции добавлять без усложнения интерфейса: минимум контролов, понятные кнопки и сообщения.

## Чеклист перед коммитом
- `dotnet build` проходит без ошибок.
- Нет артефактов сборки в репозитории (`bin/`, `obj/`).
- Обновлен `README.md`, если изменился запуск или функционал.
