using Lumo.Lexing;
using Lumo.Parsing.UnitTests.Helpers;

using Xunit;

namespace Lumo.Parsing.UnitTests;

public class ParserErrorTests
{
    public static TheoryData<string, string, int> StructureErrors()
    {
        return new TheoryData<string, string, int>
        {
            // Нарушена структура программы.
            { "", "Строка 1, столбец 1: ожидается 'func', получено конец файла", 1 },
            { "main() {}", "Строка 1, столбец 1: ожидается 'func', получено 'main'", 1 },
            { "func 5() {}", "Строка 1, столбец 6: ожидается идентификатор, получено '5'", 6 },
            { "func other() {}", "Строка 1, столбец 6: ожидается функция main", 6 },
            { "func main(x) {}", "Строка 1, столбец 11: ожидается ')', получено 'x'", 11 },
            { "func main()", "Строка 1, столбец 12: ожидается '{', получено конец файла", 12 },
            { "func main() {", "Строка 1, столбец 14: ожидается '}', получено конец файла", 14 },
            { "func main() {} func main() {}", "Строка 1, столбец 16: ожидается конец файла, получено 'func'", 16 },
        };
    }

    public static TheoryData<string, string, int> SyntaxErrors()
    {
        return new TheoryData<string, string, int>
        {
            // Нет точки с запятой, операнда или закрывающей скобки.
            { "func main() { print_int(1) }", "Строка 1, столбец 28: ожидается ';', получено '}'", 28 },
            { "func main() { 1 + ; }", "Строка 1, столбец 19: ожидается число, вызов функции или выражение в скобках", 19 },
            { "func main() { print_int(1, ); }", "Строка 1, столбец 28: ожидается число, вызов функции или выражение в скобках", 28 },
            { "func main() { print_int((1 + 2); }", "Строка 1, столбец 32: ожидается ')', получено ';'", 32 },

            // Токен, с которого не может начинаться выражение.
            { "func main() { ; }", "Строка 1, столбец 15: ожидается число, вызов функции или выражение в скобках", 15 },
            { "func main() { +2; }", "Строка 1, столбец 15: ожидается число, вызов функции или выражение в скобках", 15 },

            // После имени функции обязательны скобки вызова.
            { "func main() { read_int; }", "Строка 1, столбец 23: ожидается '(', получено ';'", 23 },
        };
    }

    public static TheoryData<string, string, int> CallErrors()
    {
        return new TheoryData<string, string, int>
        {
            // Неизвестная функция; ошибка указывает на имя функции.
            { "func main() { unknown(); }", "Строка 1, столбец 15: неизвестная функция 'unknown'", 15 },
            { "func main() { main(); }", "Строка 1, столбец 15: неизвестная функция 'main'", 15 },

            // Неверное число аргументов.
            { "func main() { read_int(2); }", "Строка 1, столбец 15: функция read_int ожидает аргументов: 0", 15 },
            { "func main() { print_int(); }", "Строка 1, столбец 15: функция print_int ожидает аргументов: 1", 15 },
            { "func main() { print_int(2, 3); }", "Строка 1, столбец 15: функция print_int ожидает аргументов: 1", 15 },
        };
    }

    public static TheoryData<string, string, int> MissingValueErrors()
    {
        return new TheoryData<string, string, int>
        {
            // Вызов print_int как аргумент: ошибка указывает на начало аргумента.
            { "func main() { print_int(print_int(2)); }", "Строка 1, столбец 25: print_int не возвращает значение", 25 },

            // Вызов print_int как операнд: ошибка указывает на знак операции.
            { "func main() { print_int(2) + 3; }", "Строка 1, столбец 28: print_int не возвращает значение", 28 },
            { "func main() { 3 * print_int(2); }", "Строка 1, столбец 17: print_int не возвращает значение", 17 },
            { "func main() { -print_int(2); }", "Строка 1, столбец 15: print_int не возвращает значение", 15 },

            // Скобки не скрывают отсутствие значения.
            { "func main() { (print_int(1)) + 1; }", "Строка 1, столбец 30: print_int не возвращает значение", 30 },
        };
    }

    [Theory]
    [MemberData(nameof(StructureErrors))]
    public void Rejects_broken_program_structure(string source, string expectedMessage, int expectedColumn)
    {
        CompilationException error = ParserRunner.Error(source);

        Assert.Equal(expectedMessage, error.Message);
        Assert.Equal(new SourcePosition(1, expectedColumn), error.Position);
    }

    [Theory]
    [MemberData(nameof(SyntaxErrors))]
    public void Rejects_syntax_error_in_statement(string source, string expectedMessage, int expectedColumn)
    {
        CompilationException error = ParserRunner.Error(source);

        Assert.Equal(expectedMessage, error.Message);
        Assert.Equal(new SourcePosition(1, expectedColumn), error.Position);
    }

    [Theory]
    [MemberData(nameof(CallErrors))]
    public void Rejects_unknown_function_and_wrong_argument_count(string source, string expectedMessage, int expectedColumn)
    {
        CompilationException error = ParserRunner.Error(source);

        Assert.Equal(expectedMessage, error.Message);
        Assert.Equal(new SourcePosition(1, expectedColumn), error.Position);
    }

    [Theory]
    [MemberData(nameof(MissingValueErrors))]
    public void Rejects_print_int_used_as_value(string source, string expectedMessage, int expectedColumn)
    {
        CompilationException error = ParserRunner.Error(source);

        Assert.Equal(expectedMessage, error.Message);
        Assert.Equal(new SourcePosition(1, expectedColumn), error.Position);
    }

    [Fact]
    public void Reports_line_and_column_of_error_on_later_line()
    {
        CompilationException error = ParserRunner.Error("func main() {\n  1 + ;\n}");

        Assert.Equal("Строка 2, столбец 7: ожидается число, вызов функции или выражение в скобках", error.Message);
        Assert.Equal(new SourcePosition(2, 7), error.Position);
    }
}