# Lumo

Компилятор учебного языка **Lumo**. На текущей итерации реализован лексический анализатор.

Спецификация языка: [docs/specification.md](docs/specification.md).

## Требования

.NET 10 SDK (C# 14): https://dotnet.microsoft.com/en-us/download/dotnet/10.0

## Структура проекта

| Каталог | Назначение |
|---------|------------|
| `src/Lumo.Lexing` | Модуль лексического анализа: лексер, токены, таблица ключевых слов |
| `src/Lumo.Cli` | Консольная утилита `lumo-lexer`, печатающая список токенов файла |
| `tests/Lumo.Lexing.UnitTests` | Модульные тесты лексического анализатора |
| `docs` | Спецификация языка |
| `examples` | Примеры программ на Lumo |

## Сборка и запуск

```bash
# Сборка
dotnet build

# Запуск тестов
dotnet test

# Разбор примера на токены
dotnet run --project src/Lumo.Cli -- examples/sample.lumo
```

Утилита печатает по одному токену в строке в формате `строка:столбец КАТЕГОРИЯ 'лексема'`.
При лексической ошибке она печатает сообщение в стандартный поток ошибок и возвращает код 1.

## Анализ покрытия кода тестами

```bash
# Однократная установка инструментов из .config/dotnet-tools.json
dotnet tool restore

scripts/run-tests-with-coverage
```

HTML-отчёт появится в `tests/coverage-report/index.html`.