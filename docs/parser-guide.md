# Парсер второй итерации: запуск и разбор

Основа: ветка `iteration_2`, коммит `3902f5a`.

## Что добавлено

- `src/Lumo.Parsing/Parser.cs`: рекурсивный спуск по EBNF.
- `Interpreter.cs`: проверочный проход, затем проход с выполнением.
- `InterpreterException.cs`: ошибки с позицией в исходном тексте.
- CLI теперь выполняет программу, а не печатает токены.
- 52 приёмочных теста с подменяемыми потоками; 186 прежних тестов лексера сохранены.

## Как запустить

Нужен .NET 10 SDK. Из папки с `Lumo.slnx`:

```powershell
dotnet build
dotnet test
dotnet run --project src/Lumo.Cli -- examples/expressions.lumo
```

Введите `5` и нажмите Enter. Ожидаемый вывод: `11`, `1`, `-3`, `-1`, каждое число на своей строке.

## Как устроен код

`Program.Run` читает файл и вызывает `Interpreter.Run`. Потоки ввода, вывода и ошибок передаются параметрами, поэтому тесты работают без настоящей консоли.

`Interpreter.Run` дважды запускает один и тот же парсер:

1. `execute: false` — проверяет синтаксис, имена функций, количество аргументов и наличие числового значения. Арифметика не выполняется, ввод не читается, вывод не производится.
2. `execute: true` — повторяет разбор и сразу выполняет операции.

Это позволяет одновременно обойтись без AST и выполнить обещание спецификации: ошибка компиляции не должна допускать даже частичное выполнение программы. Ошибки выполнения останавливают уже запущенную программу; прежний вывод сохраняется.

| Правило EBNF | Метод | Назначение |
|---|---|---|
| program | ParseProgram | Функция main и конец файла |
| main_function | ParseMainFunction | Заголовок `func main()` и блок |
| block | ParseBlock | Фигурные скобки и список инструкций |
| statement | ParseStatement | Выражение и точка с запятой |
| expression | ParseExpression | Переход к сложению/вычитанию |
| additive_expression | ParseAdditiveExpression | `+` и `-`, слева направо |
| multiplicative_expression | ParseMultiplicativeExpression | `*`, `/`, `%`, слева направо |
| unary_expression | ParseUnaryExpression | Унарный минус |
| primary_expression | ParsePrimaryExpression | Число, вызов функции или скобки |
| function_call | ParseFunctionCall | Вызовы read_int и print_int |
| argument_list | ParseArgumentList | Аргументы, разделённые запятыми |

`long?` означает число типа `long` либо отсутствие значения (`null`). Именно `null` возвращает разбор `print_int`. Метод `RequireInt` запрещает использовать такой результат внутри арифметики или как аргумент другой функции.

Умножение имеет больший приоритет, потому что `ParseAdditiveExpression` получает каждый операнд из `ParseMultiplicativeExpression`. Например, в `2 + 3 * 4` второй операнд сложения вычисляется как `3 * 4`.

## Проверка

Проверено на .NET 10: 52 новых и 186 прежних тестов прошли. Покрытие строк Parser — 99,42%. Сборка после исправления замечаний анализаторов прошла без предупреждений.

Команда для повторного измерения покрытия:

```powershell
dotnet test --collect:"XPlat Code Coverage" --results-directory tests/coverage
```

В XML-отчёте `coverage.cobertura.xml` найдите класс `Lumo.Parsing.Parser` и поле `line-rate` (умножить на 100).

## Перенос в вашу копию Git

В архиве есть полный исходный проект `Lumo/` (без служебной папки `.git`, сборок и зависимостей) и файл изменений `iteration-2-parser.patch`.

Если ваша копия репозитория уже существует, удобнее применить patch. Сначала убедитесь через `git status`, что нет незакоммиченных изменений. Если они есть, сохраните их отдельно или согласуйте перенос — не стирайте их.

Из корня вашей чистой копии репозитория:

```powershell
git fetch origin
git switch -c feature/iteration-2-parser origin/iteration_2
git apply --check "D:/путь/к/iteration-2-parser.patch"
git apply "D:/путь/к/iteration-2-parser.patch"
dotnet test
git diff --stat
git status
```

Замените путь к patch на настоящий. Если `git apply --check` сообщает конфликт, не применяйте patch принудительно: изменения одногруппника нужно совместить отдельно.

После проверки изменений и запуска у себя:

```powershell
git add Lumo.slnx README.md src/Lumo.Cli src/Lumo.Parsing tests/Lumo.Interpreter.Tests docs/parser-guide.md
git commit -m "Add entry point and expression parser with interpreter"
git push -u origin feature/iteration-2-parser
```

Затем создайте Pull Request из этой ветки в `iteration_2` для совместного просмотра. Автоматическая отправка изменений на GitHub при подготовке архива не выполнялась.
