using System.Globalization;

using Lumo.Parsing.IntegrationTests.Helpers;

using Xunit;

namespace Lumo.Parsing.IntegrationTests;

public class CompilationErrorTests
{
    public static TheoryData<string, string> InvalidPrograms()
    {
        return new TheoryData<string, string>
        {
            // Нарушена структура программы.
            { "", "Строка 1, столбец 1: ожидается 'func', получено конец файла" },
            { "main() {}", "Строка 1, столбец 1: ожидается 'func', получено 'main'" },
            { "func 5() {}", "Строка 1, столбец 6: ожидается идентификатор, получено '5'" },
            { "func other() {}", "Строка 1, столбец 6: ожидается функция main" },
            { "func main(x) {}", "Строка 1, столбец 11: ожидается ')', получено 'x'" },
            { "func main()", "Строка 1, столбец 12: ожидается '{', получено конец файла" },
            { "func main() {", "Строка 1, столбец 14: ожидается '}', получено конец файла" },
            { "func main() { print_int(1);", "Строка 1, столбец 28: ожидается '}', получено конец файла" },
            { "func main() {} func main() {}", "Строка 1, столбец 16: ожидается конец файла, получено 'func'" },

            // Номер столбца отсчитывается от начала строки, в которой найдена ошибка.
            { "func main() {\n  1 + ;\n}", "Строка 2, столбец 7: ожидается число, вызов функции или выражение в скобках" },
        };
    }

    public static TheoryData<string, string> InvalidStatements()
    {
        return new TheoryData<string, string>
        {
            // Синтаксические ошибки.
            { "print_int(1)", "Строка 4, столбец 1: ожидается ';', получено '}'" },
            { "print_int(1 + * 2);", "Строка 3, столбец 15: ожидается число, вызов функции или выражение в скобках" },
            { "print_int((1 + 2);", "Строка 3, столбец 18: ожидается ')', получено ';'" },
            { "print_int(1, );", "Строка 3, столбец 14: ожидается число, вызов функции или выражение в скобках" },
            { ";", "Строка 3, столбец 1: ожидается число, вызов функции или выражение в скобках" },
            { "+2;", "Строка 3, столбец 1: ожидается число, вызов функции или выражение в скобках" },
            { "true;", "Строка 3, столбец 1: ожидается число, вызов функции или выражение в скобках" },
            { "\"текст\";", "Строка 3, столбец 1: ожидается число, вызов функции или выражение в скобках" },
            { "read_int;", "Строка 3, столбец 9: ожидается '(', получено ';'" },
            { "x = 1;", "Строка 3, столбец 3: ожидается '(', получено '='" },

            // Лексическая ошибка.
            { "@", "Строка 3, столбец 1: недопустимый символ '@'" },

            // Вызов неизвестной функции или вызов с неверным числом аргументов.
            { "unknown();", "Строка 3, столбец 1: неизвестная функция 'unknown'" },
            { "main();", "Строка 3, столбец 1: неизвестная функция 'main'" },
            { "read_int(2);", "Строка 3, столбец 1: функция read_int ожидает аргументов: 0" },
            { "print_int();", "Строка 3, столбец 1: функция print_int ожидает аргументов: 1" },
            { "print_int(2, 3);", "Строка 3, столбец 1: функция print_int ожидает аргументов: 1" },

            // Вызов print_int в качестве операнда или аргумента.
            { "print_int(print_int(2));", "Строка 3, столбец 11: print_int не возвращает значение" },
            { "print_int(2) + 3;", "Строка 3, столбец 14: print_int не возвращает значение" },
            { "3 * print_int(2);", "Строка 3, столбец 3: print_int не возвращает значение" },
            { "-print_int(2);", "Строка 3, столбец 1: print_int не возвращает значение" },
        };
    }

    [Theory]
    [MemberData(nameof(InvalidPrograms))]
    public void Rejects_invalid_program(string source, string expectedError)
    {
        CliResult result = CliRunner.RunProgram(source, "");

        Assert.Equal(CliResult.Failure("", expectedError), result);
    }

    [Theory]
    [MemberData(nameof(InvalidStatements))]
    public void Rejects_invalid_statement_before_execution(string statement, string expectedError)
    {
        // Инструкция перед ошибочной корректна, но выполниться не должна.
        string source = "func main() {\nprint_int(1);\n" + statement + "\n}";

        CliResult result = CliRunner.RunProgram(source, "");

        Assert.Equal(CliResult.Failure("", expectedError), result);
    }

    [Fact]
    public void Does_not_read_input_when_program_has_errors()
    {
        const string source = "func main() { read_int(); unknown(); }";
        using StringReader input = new("123");
        using StringWriter output = new(CultureInfo.InvariantCulture);

        Assert.Throws<InterpreterException>(() => Interpreter.Run(source, input, output));

        Assert.Equal("123", input.ReadToEnd());
        Assert.Empty(output.ToString());
    }
}