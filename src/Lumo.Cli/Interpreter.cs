using Lumo.Ast;
using Lumo.Execution;
using Lumo.Parsing;

namespace Lumo.Cli;

/// <summary>
/// Конвейер интерпретатора: парсер строит AST, анализатор проверяет его, затем вычислитель выполняет.
/// Выполнение начинается только после успешного разбора, поэтому ошибки компиляции
/// обнаруживаются до первого ввода и вывода.
/// </summary>
public static class Interpreter
{
    public static void Run(string source, IEnvironment environment)
    {
        Parser parser = new(source);
        ProgramNode program = parser.ParseProgram();
        new SemanticAnalyzer().Check(program);
        AstEvaluator evaluator = new(environment);
        evaluator.Execute(program);
    }
}