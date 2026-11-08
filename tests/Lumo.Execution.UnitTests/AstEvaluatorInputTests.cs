using Lumo.Ast;
using Lumo.Execution.UnitTests.Helpers;
using Lumo.Lexing;

using Xunit;

using static Lumo.Execution.UnitTests.Helpers.AstFactory;

namespace Lumo.Execution.UnitTests;

public class AstEvaluatorInputTests
{
    public static TheoryData<string, string> AcceptedLines()
    {
        return new TheoryData<string, string>
        {
            // Знак необязателен, ведущие нули допустимы.
            { "5", "5\n" },
            { "+5", "5\n" },
            { "-5", "-5\n" },
            { "007", "7\n" },

            // Пробелы и табуляции по краям строки игнорируются.
            { "  7\t", "7\n" },

            // Границы диапазона.
            { "9223372036854775807", "9223372036854775807\n" },
            { "-9223372036854775808", "-9223372036854775808\n" },
        };
    }

    public static TheoryData<string> MalformedLines()
    {
        return new TheoryData<string>
        {
            "",
            "   ",
            "+",
            "-",
            "- 5",
            "1.5",
            "abc",
            "5\0",
        };
    }

    public static TheoryData<string> OutOfRangeLines()
    {
        return new TheoryData<string>
        {
            "9223372036854775808",
            "-9223372036854775809",
        };
    }

    [Theory]
    [MemberData(nameof(AcceptedLines))]
    public void Reads_integer_from_input_line(string line, string expectedOutput)
    {
        ProgramNode program = ProgramOf(Exec(PrintInt(ReadInt())));
        FakeEnvironment environment = new(line);

        new AstEvaluator(environment).Execute(program);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Theory]
    [MemberData(nameof(MalformedLines))]
    public void Rejects_line_that_is_not_decimal_integer(string line)
    {
        ProgramNode program = ProgramOf(Exec(ReadInt(new SourcePosition(2, 5))));
        FakeEnvironment environment = new(line);

        RuntimeErrorException error = Assert.Throws<RuntimeErrorException>(
            () => new AstEvaluator(environment).Execute(program));

        Assert.Equal("Строка 2, столбец 5: ожидается десятичная запись целого числа", error.Message);
    }

    [Theory]
    [MemberData(nameof(OutOfRangeLines))]
    public void Rejects_integer_outside_of_range(string line)
    {
        ProgramNode program = ProgramOf(Exec(ReadInt(new SourcePosition(2, 5))));
        FakeEnvironment environment = new(line);

        RuntimeErrorException error = Assert.Throws<RuntimeErrorException>(
            () => new AstEvaluator(environment).Execute(program));

        Assert.Equal("Строка 2, столбец 5: ожидается целое число в диапазоне int", error.Message);
    }

    [Fact]
    public void Rejects_exhausted_input()
    {
        ProgramNode program = ProgramOf(Exec(ReadInt(new SourcePosition(2, 5))));
        FakeEnvironment environment = new();

        RuntimeErrorException error = Assert.Throws<RuntimeErrorException>(
            () => new AstEvaluator(environment).Execute(program));

        Assert.Equal("Строка 2, столбец 5: поток ввода исчерпан", error.Message);
    }
}