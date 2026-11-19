using Lumo.Cli.AcceptanceTests.Helpers;

using Xunit;

namespace Lumo.Cli.AcceptanceTests;

public class VariableFunctionTests
{
    public static TheoryData<string, string> NumericBuiltinCalls()
    {
        return new TheoryData<string, string>
        {
            { "min(9)", "9\n" },
            { "max(-2)", "-2\n" },
            { "max(-9, -7, -4, -12, -5)", "-4\n" },
            { "min(11, -4, 0, 8, 9)", "-4\n" },
            { "abs(-8)", "8\n" },
        };
    }

    public static TheoryData<string, string> InvalidDeclarations()
    {
        return new TheoryData<string, string>
        {
            { "func f() { var x: int; }", "ожидается '='" },
            { "func f() { var x = 3; }", "ожидается ':'" },
            { "func f() { var x: string = 3; }", "ожидается 'int'" },
            { "func f() { var x: int = x + 1; }", "неизвестная переменная 'x'" },
            { "func f() { x = 5; }", "неизвестная переменная 'x'" },
            { "func f() { { var x: int = 1; } print_int(x); }", "неизвестная переменная 'x'" },
            { "func f() { var x: int = 1; var x: int = 2; }", "повторное объявление 'x'" },
            { "func f(x: int) { var x: int = 2; }", "повторное объявление 'x'" },
            { "func f(x: int, x: int) {}", "повторное объявление 'x'" },
            { "func f() {} func f() {}", "повторное объявление функции 'f'" },
            { "func max() {}", "встроенной функции 'max'" },
            { "func f() -> int {}", "нет возврата значения" },
            { "func f() -> int { return; }", "ожидается возвращаемое значение" },
            { "func f() { return 1; }", "не возвращает значение" },
            { "func f() {} func g() { var x: int = f(); }", "f не возвращает значение" },
            { "func f() {} func g() -> int { return f(); }", "f не возвращает значение" },
            { "func f() {} func g() { max(f(), 1); }", "f не возвращает значение" },
            { "func f(a: int) {} func g() { f(); }", "функция f ожидает аргументов: 1" },
            { "func f() { min(); }", "хотя бы один аргумент" },
            { "func f() { max(); }", "хотя бы один аргумент" },
            { "func f() { abs(1, 2); }", "функция abs ожидает аргументов: 1" },
            { "func f() { 1 + 2 = 5; }", "слева от '='" },
            { "func f() { var a: int = 1; var b: int = 2; a = b = 1; }", "ожидается ';', получено '='" },
        };
    }

    public static TheoryData<string> InvalidEntryPoints()
    {
        return new TheoryData<string>
        {
            "func main(x: int) {}",
            "func main() -> int { return 0; }",
        };
    }

    [Fact]
    public void Arguments_are_evaluated_once_from_left_to_right_in_the_caller()
    {
        const string source = """
            func trace(x: int) -> int { print_int(x); return x; }
            func digits(a: int, b: int) -> int { return 10 * a + b; }
            func main() { print_int(digits(trace(read_int()), trace(read_int()))); }
            """;

        Assert.Equal(new CliResult(0, "4\n2\n42\n", ""), CliRunner.RunProgram(source, "4\n2\n"));
    }

    [Fact]
    public void A_block_can_shadow_a_name_and_assign_to_an_outer_variable()
    {
        const string source = """
            func main() {
                var x: int = 4;
                var y: int = 0;
                { var x: int = x + 1; { y = x * 2; } x = 9; print_int(x); }
                print_int(x);
                print_int(y);
            }
            """;

        Assert.Equal(new CliResult(0, "9\n4\n10\n", ""), CliRunner.RunProgram(source, ""));
    }

    [Fact]
    public void Calls_have_fresh_locals_and_parameters_are_passed_by_value()
    {
        const string source = """
            func main() {
                var value: int = 3;
                var bump: int = 8;
                print_int(bump(value) + bump(value));
                print_int(value);
                print_int(bump);
            }
            func bump(value: int) -> int { value = value + 1; return value; }
            """;

        Assert.Equal(new CliResult(0, "8\n3\n8\n", ""), CliRunner.RunProgram(source, ""));
    }

    [Fact]
    public void Return_leaves_nested_blocks_and_only_ends_the_current_call()
    {
        const string source = """
            func answer() -> int { { return 42; } print_int(91); }
            func stop() { { return; } print_int(92); }
            func show() { print_int(answer()); }
            func main() { stop(); show(); print_int(7); }
            """;

        Assert.Equal(new CliResult(0, "42\n7\n", ""), CliRunner.RunProgram(source, ""));
    }

    [Theory]
    [MemberData(nameof(NumericBuiltinCalls))]
    public void Numeric_builtins_return_integers(string expression, string expected)
    {
        CliResult result = CliRunner.RunProgram("func main() { print_int(" + expression + "); }", "");

        Assert.Equal(new CliResult(0, expected, ""), result);
    }

    [Theory]
    [MemberData(nameof(InvalidDeclarations))]
    public void Invalid_declarations_are_rejected_even_if_the_function_is_not_called(string declaration, string message)
    {
        CliResult result = CliRunner.RunProgram("func main() { print_int(7); }\n" + declaration, "");

        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains(message, result.Error);
    }

    [Fact]
    public void Assignment_target_may_be_parenthesized()
    {
        const string source = """
            func main() { var x: int = 1; (x) = 5; print_int(x); }
            """;

        Assert.Equal(new CliResult(0, "5\n", ""), CliRunner.RunProgram(source, ""));
    }

    [Fact]
    public void Caller_locals_are_not_visible_inside_the_callee()
    {
        const string source = """
            func main() { var x: int = 3; print_int(1); show(); }
            func show() { print_int(x); }
            """;
        CliResult result = CliRunner.RunProgram(source, "");

        Assert.Equal(CliResult.Failure("", "Строка 2, столбец 25: неизвестная переменная 'x'"), result);
    }

    [Fact]
    public void Abs_overflow_is_a_runtime_error_and_keeps_earlier_output()
    {
        CliResult result = CliRunner.RunProgram("func main() { print_int(1); abs(read_int()); }", "-9223372036854775808");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("1\n", result.Output);
        Assert.Contains("переполнение", result.Error);
    }

    [Theory]
    [MemberData(nameof(InvalidEntryPoints))]
    public void Entry_point_has_no_parameters_or_result(string source)
    {
        CliResult result = CliRunner.RunProgram(source, "");

        Assert.Equal(CliResult.Failure("", "Строка 1, столбец 6: точка входа должна иметь вид func main()"), result);
    }

    [Fact]
    public void Unbounded_mutual_recursion_stops_with_a_language_error()
    {
        const string source = """
            func main() { print_int(1); first(); }
            func first() { second(); }
            func second() { first(); }
            """;
        CliResult result = CliRunner.RunProgram(source, "");

        Assert.Equal(1, result.ExitCode);
        Assert.Equal("1\n", result.Output);
        Assert.Contains("глубина вызовов", result.Error);
    }
}