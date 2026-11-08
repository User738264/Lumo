using Lumo.Ast;
using Lumo.Execution.UnitTests.Helpers;

using Xunit;

using static Lumo.Execution.UnitTests.Helpers.AstFactory;

namespace Lumo.Execution.UnitTests;

public class AstEvaluatorTests
{
    public static TheoryData<BinaryOperator, long, long, string> BinaryCases()
    {
        return new TheoryData<BinaryOperator, long, long, string>
        {
            // Сложение, вычитание и умножение.
            { BinaryOperator.Add, 2, 3, "5\n" },
            { BinaryOperator.Subtract, 2, 5, "-3\n" },
            { BinaryOperator.Multiply, 4, -3, "-12\n" },

            // Деление нацело отбрасывает дробную часть.
            { BinaryOperator.Divide, 7, 2, "3\n" },
            { BinaryOperator.Divide, -7, 2, "-3\n" },
            { BinaryOperator.Divide, 7, -2, "-3\n" },

            // Остаток имеет знак делимого.
            { BinaryOperator.Remainder, 7, 3, "1\n" },
            { BinaryOperator.Remainder, -7, 3, "-1\n" },
            { BinaryOperator.Remainder, 7, -3, "1\n" },

            // Остаток при делителе -1 равен нулю, даже для наименьшего числа.
            { BinaryOperator.Remainder, long.MinValue, -1, "0\n" },

            // Результат на границе диапазона ещё не переполнение.
            { BinaryOperator.Add, long.MaxValue - 1, 1, "9223372036854775807\n" },
        };
    }

    [Theory]
    [MemberData(nameof(BinaryCases))]
    public void Applies_binary_operator(BinaryOperator operation, long left, long right, string expectedOutput)
    {
        ProgramNode program = ProgramOf(Exec(PrintInt(Binary(operation, Literal(left), Literal(right)))));
        FakeEnvironment environment = new();

        new AstEvaluator(environment).Execute(program);

        Assert.Equal(expectedOutput, environment.Output);
    }

    [Fact]
    public void Negates_operand()
    {
        ProgramNode program = ProgramOf(Exec(PrintInt(Negate(Literal(5)))));
        FakeEnvironment environment = new();

        new AstEvaluator(environment).Execute(program);

        Assert.Equal("-5\n", environment.Output);
    }

    [Fact]
    public void Follows_tree_shape_instead_of_operator_priority()
    {
        // Дерево (1 + 2) * 3: вложенная сумма вычисляется раньше умножения.
        Expression sum = Binary(BinaryOperator.Add, Literal(1), Literal(2));
        ProgramNode program = ProgramOf(Exec(PrintInt(Binary(BinaryOperator.Multiply, sum, Literal(3)))));
        FakeEnvironment environment = new();

        new AstEvaluator(environment).Execute(program);

        Assert.Equal("9\n", environment.Output);
    }

    [Fact]
    public void Evaluates_left_operand_before_right_operand()
    {
        // Если бы правый операнд вычислялся первым, результат был бы -7.
        Expression difference = Binary(BinaryOperator.Subtract, ReadInt(), ReadInt());
        ProgramNode program = ProgramOf(Exec(PrintInt(difference)));
        FakeEnvironment environment = new("10", "3");

        new AstEvaluator(environment).Execute(program);

        Assert.Equal("7\n", environment.Output);
    }

    [Fact]
    public void Prints_each_value_on_its_own_line_with_lf()
    {
        ProgramNode program = ProgramOf(Exec(PrintInt(Literal(1))), Exec(PrintInt(Literal(-2))));
        FakeEnvironment environment = new();

        new AstEvaluator(environment).Execute(program);

        Assert.Equal("1\n-2\n", environment.Output);
    }

    [Fact]
    public void Produces_no_output_for_empty_main()
    {
        ProgramNode program = ProgramOf();
        FakeEnvironment environment = new();

        new AstEvaluator(environment).Execute(program);

        Assert.Equal(string.Empty, environment.Output);
    }

    [Fact]
    public void Discards_value_of_expression_statement()
    {
        ProgramNode program = ProgramOf(
            Exec(Literal(5)),
            Exec(Binary(BinaryOperator.Add, Literal(1), Literal(2))),
            Exec(PrintInt(Literal(7))));
        FakeEnvironment environment = new();

        new AstEvaluator(environment).Execute(program);

        Assert.Equal("7\n", environment.Output);
    }

    [Fact]
    public void Reads_input_even_when_value_is_discarded()
    {
        // Первая строка ввода уходит в отброшенное значение, печатается вторая.
        ProgramNode program = ProgramOf(Exec(ReadInt()), Exec(PrintInt(ReadInt())));
        FakeEnvironment environment = new("1", "2");

        new AstEvaluator(environment).Execute(program);

        Assert.Equal("2\n", environment.Output);
    }

    [Fact]
    public void Reads_one_line_per_read_int_call()
    {
        ProgramNode program = ProgramOf(Exec(PrintInt(ReadInt())));
        FakeEnvironment environment = new("4", "5", "6");

        new AstEvaluator(environment).Execute(program);

        Assert.Equal(2, environment.UnreadLineCount);
    }
}