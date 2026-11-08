using Lumo.Ast;
using Lumo.Execution.UnitTests.Helpers;
using Lumo.Lexing;

using Xunit;

using static Lumo.Execution.UnitTests.Helpers.AstFactory;

namespace Lumo.Execution.UnitTests;

public class AstEvaluatorErrorTests
{
    public static TheoryData<BinaryOperator, long, long, string> FailingBinaryCases()
    {
        return new TheoryData<BinaryOperator, long, long, string>
        {
            // Деление и взятие остатка при нулевом делителе.
            { BinaryOperator.Divide, 1, 0, "деление на ноль" },
            { BinaryOperator.Remainder, 1, 0, "деление на ноль" },
            { BinaryOperator.Divide, 0, 0, "деление на ноль" },

            // Результат вне диапазона типа.
            { BinaryOperator.Add, long.MaxValue, 1, "целочисленное переполнение" },
            { BinaryOperator.Subtract, long.MinValue, 1, "целочисленное переполнение" },
            { BinaryOperator.Multiply, long.MaxValue, 2, "целочисленное переполнение" },
            { BinaryOperator.Divide, long.MinValue, -1, "целочисленное переполнение" },
        };
    }

    [Theory]
    [MemberData(nameof(FailingBinaryCases))]
    public void Reports_binary_operation_error_at_operator_position(
        BinaryOperator operation,
        long left,
        long right,
        string description)
    {
        Expression expression = Binary(operation, Literal(left), Literal(right), new SourcePosition(3, 7));
        ProgramNode program = ProgramOf(Exec(expression));
        FakeEnvironment environment = new();

        RuntimeErrorException error = Assert.Throws<RuntimeErrorException>(
            () => new AstEvaluator(environment).Execute(program));

        Assert.Equal($"Строка 3, столбец 7: {description}", error.Message);
        Assert.Equal(new SourcePosition(3, 7), error.Position);
    }

    [Fact]
    public void Reports_negation_overflow_at_operator_position()
    {
        Expression expression = Negate(Literal(long.MinValue), new SourcePosition(3, 1));
        ProgramNode program = ProgramOf(Exec(expression));
        FakeEnvironment environment = new();

        RuntimeErrorException error = Assert.Throws<RuntimeErrorException>(
            () => new AstEvaluator(environment).Execute(program));

        Assert.Equal("Строка 3, столбец 1: целочисленное переполнение", error.Message);
    }

    [Fact]
    public void Keeps_earlier_output_and_skips_later_statements_after_error()
    {
        Expression failing = Binary(BinaryOperator.Divide, Literal(1), Literal(0));
        ProgramNode program = ProgramOf(
            Exec(PrintInt(Literal(7))),
            Exec(PrintInt(failing)),
            Exec(PrintInt(Literal(9))));
        FakeEnvironment environment = new();

        Assert.Throws<RuntimeErrorException>(() => new AstEvaluator(environment).Execute(program));

        Assert.Equal("7\n", environment.Output);
    }

    [Fact]
    public void Evaluates_both_operands_before_reporting_operation_error()
    {
        // Деление на ноль обнаруживается при применении оператора, когда оба операнда уже вычислены.
        Expression expression = Binary(BinaryOperator.Divide, Literal(1), Binary(BinaryOperator.Subtract, ReadInt(), ReadInt()));
        ProgramNode program = ProgramOf(Exec(expression));
        FakeEnvironment environment = new("4", "4", "99");

        Assert.Throws<RuntimeErrorException>(() => new AstEvaluator(environment).Execute(program));

        Assert.Equal(1, environment.UnreadLineCount);
    }

    [Fact]
    public void Rejects_call_of_function_unknown_to_evaluator()
    {
        // Парсер такое дерево не построит; вычислитель рассчитан на проверенные деревья и сообщает о нарушении договора.
        ProgramNode program = ProgramOf(Exec(Call("unknown")));
        FakeEnvironment environment = new();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => new AstEvaluator(environment).Execute(program));

        Assert.Equal("Неизвестная функция 'unknown'", error.Message);
    }
}