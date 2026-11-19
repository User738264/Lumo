using Lumo.Ast;

using Xunit;

namespace Lumo.Parsing.UnitTests.Helpers;

/// <summary>
///  Вспомогательные методы для запуска парсера в тестах.
/// </summary>
public static class ParserRunner
{
    // Разбирает программу целиком.
    public static ProgramNode Parse(string source)
    {
        Parser parser = new(source);
        return parser.ParseProgram();
    }

    // Разбирает выражение, помещённое в единственную инструкцию функции main, и печатает его AST.
    public static string PrintExpression(string expression)
    {
        ProgramNode program = Parse("func main() { " + expression + "; }");
        ExpressionStatement statement = Assert.IsType<ExpressionStatement>(
            Assert.Single(program.Functions.Single(function => function.Name == EntryPoint.Name).Body.Statements));
        return AstPrinter.Print(statement.Expression);
    }

    // Проверяет разбор и семантический анализ, возвращая ошибку компиляции.
    public static CompilationException Error(string source)
    {
        return Assert.Throws<CompilationException>(() =>
        {
            ProgramNode program = Parse(source);
            new SemanticAnalyzer().Check(program);
        });
    }
}