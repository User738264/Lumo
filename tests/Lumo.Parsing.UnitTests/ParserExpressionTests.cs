using Lumo.Parsing.UnitTests.Helpers;

using Xunit;

namespace Lumo.Parsing.UnitTests;

public class ParserExpressionTests
{
    public static TheoryData<string, string> ExpressionCases()
    {
        return new TheoryData<string, string>
        {
            // Литералы и выражение в скобках: скобки в дереве не остаются.
            { "1", "1" },
            { "9223372036854775807", "9223372036854775807" },
            { "((1))", "1" },

            // Умножение, деление и остаток связывают сильнее сложения и вычитания.
            { "1 + 2 * 3", "(+ 1 (* 2 3))" },
            { "1 * 2 + 3", "(+ (* 1 2) 3)" },
            { "6 - 4 / 2", "(- 6 (/ 4 2))" },
            { "7 + 4 % 3", "(+ 7 (% 4 3))" },

            // Скобки меняют порядок применения операций.
            { "(1 + 2) * 3", "(* (+ 1 2) 3)" },

            // Бинарные операторы левоассоциативны.
            { "10 - 4 - 3", "(- (- 10 4) 3)" },
            { "24 / 4 / 2", "(/ (/ 24 4) 2)" },
            { "7 % 4 * 2", "(* (% 7 4) 2)" },
            { "1 + 2 - 3", "(- (+ 1 2) 3)" },

            // Унарный минус правоассоциативен и связывает сильнее бинарных операторов.
            { "-5", "(neg 5)" },
            { "--5", "(neg (neg 5))" },
            { "-2 * 3", "(* (neg 2) 3)" },
            { "-(1 + 2)", "(neg (+ 1 2))" },
            { "1 - -2", "(- 1 (neg 2))" },

            // Выражение из спецификации целиком.
            { "10 + 7 * (2 * 3 + 4) + 5 + -5", "(+ (+ (+ 10 (* 7 (+ (* 2 3) 4))) 5) (neg 5))" },
        };
    }

    public static TheoryData<string, string> CallCases()
    {
        return new TheoryData<string, string>
        {
            // Вызов без аргументов и с одним аргументом.
            { "read_int()", "(call read_int)" },
            { "print_int(1)", "(call print_int 1)" },

            // Аргументом может быть любое выражение, в том числе вызов read_int.
            { "print_int(read_int() + 1)", "(call print_int (+ (call read_int) 1))" },

            // Вызов может быть операндом, а вызов print_int в скобках — инструкцией.
            { "1 + read_int() * 2", "(+ 1 (* (call read_int) 2))" },
            { "(print_int(1))", "(call print_int 1)" },
        };
    }

    [Theory]
    [MemberData(nameof(ExpressionCases))]
    public void Builds_tree_with_precedence_and_associativity(string expression, string expectedTree)
    {
        string tree = ParserRunner.PrintExpression(expression);

        Assert.Equal(expectedTree, tree);
    }

    [Theory]
    [MemberData(nameof(CallCases))]
    public void Builds_tree_for_function_calls(string expression, string expectedTree)
    {
        string tree = ParserRunner.PrintExpression(expression);

        Assert.Equal(expectedTree, tree);
    }
}