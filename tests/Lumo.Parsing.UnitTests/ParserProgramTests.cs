using Lumo.Ast;
using Lumo.Lexing;
using Lumo.Parsing.UnitTests.Helpers;

using Xunit;

namespace Lumo.Parsing.UnitTests;

public class ParserProgramTests
{
    public static TheoryData<string, string> ProgramCases()
    {
        return new TheoryData<string, string>
        {
            // Пустая функция main.
            { "func main() {}", "func main { }" },

            // Узлы параметров, чтения, объявления, присваивания и обе формы return.
            {
                "func twice(x: int) -> int { return x * 2; } func main() { var n: int = 1; n = twice(n); return; }",
                "func twice(x) -> int { return (* x 2); } func main { var n = 1; n = (call twice n); return; }"
            },

            // Несколько инструкций сохраняют порядок записи.
            { "func main() { 1; print_int(2); read_int(); }", "func main { 1; (call print_int 2); (call read_int); }" },

            // Круглые скобки вокруг цели присваивания допустимы и в дереве не остаются.
            { "func main() { var x: int = 1; (x) = 5; ((x)) = 6; }", "func main { var x = 1; x = 5; x = 6; }" },

            // Комментарии и переводы строк в дерево не попадают.
            { "/* начало */ func main() {\n  1 + 2; // сумма\n}", "func main { (+ 1 2); }" },
        };
    }

    [Theory]
    [MemberData(nameof(ProgramCases))]
    public void Builds_tree_for_program(string source, string expectedTree)
    {
        ProgramNode program = ParserRunner.Parse(source);

        Assert.Equal(expectedTree, AstPrinter.Print(program));
    }

    [Fact]
    public void Builds_tree_without_semantically_validating_variable_names()
    {
        ProgramNode program = new Parser("func main() { missing; }").ParseProgram();

        ExpressionStatement statement = Assert.IsType<ExpressionStatement>(Assert.Single(program.Functions[0].Body.Statements));
        VariableExpression variable = Assert.IsType<VariableExpression>(statement.Expression);
        Assert.Equal("missing", variable.Name);
    }

    [Fact]
    public void Allows_an_empty_program_node_to_be_constructed()
    {
        ProgramNode program = new([]);

        Assert.Empty(program.Functions);
        Assert.Equal(new SourcePosition(1, 1), program.Position);
    }

    [Fact]
    public void Stores_main_function_name_and_body()
    {
        ProgramNode program = ParserRunner.Parse("func main() { 1; 2; }");

        FunctionDeclaration main = program.Functions.Single(function => function.Name == "main");
        Assert.Equal("main", main.Name);
        Assert.Equal(2, main.Body.Statements.Count);
    }

    [Fact]
    public void Records_positions_of_declarations_and_statements()
    {
        ProgramNode program = ParserRunner.Parse("func main() {\n  1 + 2;\n}");

        FunctionDeclaration function = program.Functions.Single(function => function.Name == "main");
        Statement statement = Assert.Single(function.Body.Statements);
        Assert.Equal(new SourcePosition(1, 1), program.Position);
        Assert.Equal(new SourcePosition(1, 1), function.Position);
        Assert.Equal(new SourcePosition(1, 13), function.Body.Position);
        Assert.Equal(new SourcePosition(2, 3), statement.Position);
    }

    [Fact]
    public void Records_positions_of_expressions()
    {
        ProgramNode program = ParserRunner.Parse("func main() {\n  -1 + read_int();\n}");

        ExpressionStatement statement = Assert.IsType<ExpressionStatement>(
            Assert.Single(program.Functions.Single(function => function.Name == "main").Body.Statements));
        BinaryExpression sum = Assert.IsType<BinaryExpression>(statement.Expression);
        UnaryExpression negation = Assert.IsType<UnaryExpression>(sum.Left);
        IntLiteralExpression one = Assert.IsType<IntLiteralExpression>(negation.Operand);
        CallExpression call = Assert.IsType<CallExpression>(sum.Right);
        Assert.Equal(new SourcePosition(2, 6), sum.Position);
        Assert.Equal(new SourcePosition(2, 3), negation.Position);
        Assert.Equal(new SourcePosition(2, 4), one.Position);
        Assert.Equal(new SourcePosition(2, 8), call.Position);
    }

    [Fact]
    public void Stops_with_compilation_error_before_returning_tree()
    {
        CompilationException error = ParserRunner.Error("func main() { 1 + ; }");

        Assert.Equal("Строка 1, столбец 19: ожидается число, имя или выражение в скобках", error.Message);
        Assert.Equal(new SourcePosition(1, 19), error.Position);
    }
}