using Lumo.Ast;
using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>Проверяет готовое дерево без ввода, вывода и вычисления выражений.</summary>
internal sealed class SemanticAnalyzer
{
    private readonly Dictionary<string, FunctionDeclaration> _functions = new(StringComparer.Ordinal);
    private readonly Stack<HashSet<string>> _scopes = new();

    public void Check(ProgramNode program)
    {
        // Сначала собираем все заголовки: взаимные ссылки и вызовы до объявления допустимы.
        foreach (FunctionDeclaration function in program.Functions)
        {
            if (BuiltinFunctions.IsBuiltin(function.Name))
            {
                throw new CompilationException($"нельзя изменить объявление встроенной функции '{function.Name}'", function.NamePosition);
            }

            if (!_functions.TryAdd(function.Name, function))
            {
                throw new CompilationException($"повторное объявление функции '{function.Name}'", function.NamePosition);
            }
        }

        if (!_functions.TryGetValue("main", out FunctionDeclaration? main))
        {
            throw new CompilationException("ожидается функция main", program.Functions[0].NamePosition);
        }

        if (main.Parameters.Count != 0 || main.ReturnsValue)
        {
            throw new CompilationException("точка входа должна иметь вид func main()", main.NamePosition);
        }

        foreach (FunctionDeclaration function in program.Functions)
        {
            CheckFunction(function);
        }
    }

    private void CheckFunction(FunctionDeclaration function)
    {
        _scopes.Push(new HashSet<string>(StringComparer.Ordinal));
        foreach (Parameter parameter in function.Parameters)
        {
            Declare(parameter.Name, parameter.Position);
        }

        // Параметры и верхний блок тела имеют общую область видимости.
        bool returns = CheckStatements(function.Body, function);
        _scopes.Pop();
        if (function.ReturnsValue && !returns)
        {
            throw new CompilationException($"в функции '{function.Name}' нет возврата значения", function.NamePosition);
        }
    }

    private bool CheckStatements(BlockStatement block, FunctionDeclaration function)
    {
        bool returns = false;
        foreach (Statement statement in block.Statements)
        {
            // В языке пока нет ветвлений: достигнутый return безусловен.
            // Остальные инструкции тоже проверяем, даже если они не будут выполнены.
            if (CheckStatement(statement, function))
            {
                returns = true;
            }
        }

        return returns;
    }

    private bool CheckStatement(Statement statement, FunctionDeclaration function)
    {
        switch (statement)
        {
            case BlockStatement block:
                _scopes.Push(new HashSet<string>(StringComparer.Ordinal));
                bool returns = CheckStatements(block, function);
                _scopes.Pop();
                return returns;
            case VariableDeclarationStatement declaration:
                CheckExpression(declaration.Initializer, declaration.Initializer.Position);
                Declare(declaration.Name, declaration.Position);
                break;
            case AssignmentStatement assignment:
                RequireVariable(assignment.Name, assignment.Position);
                CheckExpression(assignment.Value, assignment.Value.Position);
                break;
            case ReturnStatement exit:
                if (exit.Value == null && function.ReturnsValue)
                {
                    throw new CompilationException("ожидается возвращаемое значение", exit.Position);
                }

                if (exit.Value != null)
                {
                    if (!function.ReturnsValue)
                    {
                        throw new CompilationException($"функция {function.Name} не возвращает значение", exit.Position);
                    }

                    CheckExpression(exit.Value, exit.Value.Position);
                }

                return true;
            case ExpressionStatement discarded:
                CheckExpression(discarded.Expression);
                break;
            default:
                throw new InvalidOperationException("Неизвестный вид инструкции");
        }

        return false;
    }

    // Позиция передаётся, только если выражение должно дать числовое значение.
    private void CheckExpression(Expression expression, SourcePosition? valueRequiredAt = null)
    {
        switch (expression)
        {
            case IntLiteralExpression:
                break;
            case VariableExpression variable:
                RequireVariable(variable.Name, variable.Position);
                break;
            case UnaryExpression unary:
                CheckExpression(unary.Operand, unary.Position);
                break;
            case BinaryExpression binary:
                CheckExpression(binary.Left, binary.Position);
                CheckExpression(binary.Right, binary.Position);
                break;
            case CallExpression call:
                bool returnsValue = CheckCall(call);
                if (!returnsValue && valueRequiredAt is SourcePosition position)
                {
                    throw new CompilationException($"{call.Callee} не возвращает значение", position);
                }

                break;
            default:
                throw new InvalidOperationException("Неизвестный вид выражения");
        }
    }

    private bool CheckCall(CallExpression call)
    {
        int count = call.Arguments.Count;
        int expected;
        bool returnsValue = true;
        switch (call.Callee)
        {
            case BuiltinFunctions.ReadInt:
                expected = 0;
                break;
            case BuiltinFunctions.PrintInt:
                returnsValue = false;
                expected = 1;
                break;
            case BuiltinFunctions.Abs:
                expected = 1;
                break;
            case BuiltinFunctions.Min or BuiltinFunctions.Max:
                if (count == 0)
                {
                    throw new CompilationException($"функция {call.Callee} ожидает хотя бы один аргумент", call.Position);
                }

                expected = count;
                break;
            case "main":
                throw new CompilationException("точку входа main нельзя вызывать явно", call.Position);
            default:
                if (!_functions.TryGetValue(call.Callee, out FunctionDeclaration? function))
                {
                    throw new CompilationException($"неизвестная функция '{call.Callee}'", call.Position);
                }

                expected = function.Parameters.Count;
                returnsValue = function.ReturnsValue;
                break;
        }

        if (count != expected)
        {
            throw new CompilationException($"функция {call.Callee} ожидает аргументов: {expected}", call.Position);
        }

        foreach (Expression argument in call.Arguments)
        {
            CheckExpression(argument, argument.Position);
        }

        return returnsValue;
    }

    private void Declare(string name, SourcePosition position)
    {
        if (!_scopes.Peek().Add(name))
        {
            throw new CompilationException($"повторное объявление '{name}' в этой области", position);
        }
    }

    private void RequireVariable(string name, SourcePosition position)
    {
        if (!_scopes.Any(scope => scope.Contains(name)))
        {
            throw new CompilationException($"неизвестная переменная '{name}'", position);
        }
    }
}