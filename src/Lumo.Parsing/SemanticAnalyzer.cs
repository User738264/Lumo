using Lumo.Ast;
using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>Проверяет готовое дерево без ввода, вывода и вычисления выражений.</summary>
public sealed class SemanticAnalyzer : IAstVisitor
{
    private readonly Dictionary<string, FunctionDeclaration> _functions = new(StringComparer.Ordinal);
    private readonly Stack<HashSet<string>> _scopes = new();
    private FunctionDeclaration? _currentFunction;
    private SourcePosition? _valueRequiredAt;
    private bool _expressionReturnsValue;
    private bool _statementReturns;

    public void Check(ProgramNode program)
    {
        _functions.Clear();
        _scopes.Clear();
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
            SourcePosition position = program.Functions.Count > 0 ? program.Functions[0].NamePosition : program.Position;
            throw new CompilationException("ожидается функция main", position);
        }

        if (main.Parameters.Count != 0 || main.ReturnsValue)
        {
            throw new CompilationException("точка входа должна иметь вид func main()", main.NamePosition);
        }

        program.Accept(this);
    }

    public void Visit(ProgramNode node)
    {
        foreach (FunctionDeclaration function in node.Functions)
        {
            function.Accept(this);
        }
    }

    public void Visit(FunctionDeclaration node)
    {
        _currentFunction = node;
        _scopes.Push(new HashSet<string>(StringComparer.Ordinal));
        try
        {
            foreach (Parameter parameter in node.Parameters)
            {
                parameter.Accept(this);
            }

            bool returns = CheckStatements(node.Body);
            if (node.ReturnsValue && !returns)
            {
                throw new CompilationException($"в функции '{node.Name}' нет возврата значения", node.NamePosition);
            }
        }
        finally
        {
            _scopes.Pop();
            _currentFunction = null;
        }
    }

    public void Visit(Parameter node)
    {
        Declare(node.Name, node.Position);
    }

    public void Visit(BlockStatement node)
    {
        _scopes.Push(new HashSet<string>(StringComparer.Ordinal));
        try
        {
            _statementReturns = CheckStatements(node);
        }
        finally
        {
            _scopes.Pop();
        }
    }

    public void Visit(ExpressionStatement node)
    {
        _ = CheckExpression(node.Expression);
        _statementReturns = false;
    }

    public void Visit(VariableDeclarationStatement node)
    {
        _ = CheckExpression(node.Initializer, node.Initializer.Position);
        Declare(node.Name, node.Position);
        _statementReturns = false;
    }

    public void Visit(AssignmentStatement node)
    {
        RequireVariable(node.Name, node.Position);
        _ = CheckExpression(node.Value, node.Value.Position);
        _statementReturns = false;
    }

    public void Visit(ReturnStatement node)
    {
        FunctionDeclaration function = _currentFunction!;
        if (node.Value == null && function.ReturnsValue)
        {
            throw new CompilationException("ожидается возвращаемое значение", node.Position);
        }

        if (node.Value != null)
        {
            if (!function.ReturnsValue)
            {
                throw new CompilationException($"функция {function.Name} не возвращает значение", node.Position);
            }

            _ = CheckExpression(node.Value, node.Value.Position);
        }

        _statementReturns = true;
    }

    public void Visit(VariableExpression node)
    {
        RequireVariable(node.Name, node.Position);
        _expressionReturnsValue = true;
    }

    public void Visit(IntLiteralExpression node)
    {
        _expressionReturnsValue = true;
    }

    public void Visit(UnaryExpression node)
    {
        _ = CheckExpression(node.Operand, node.Position);
        _expressionReturnsValue = true;
    }

    public void Visit(BinaryExpression node)
    {
        _ = CheckExpression(node.Left, node.Position);
        _ = CheckExpression(node.Right, node.Position);
        _expressionReturnsValue = true;
    }

    public void Visit(CallExpression node)
    {
        bool returnsValue = CheckCall(node);
        if (!returnsValue && _valueRequiredAt is SourcePosition position)
        {
            throw new CompilationException($"{node.Callee} не возвращает значение", position);
        }

        _expressionReturnsValue = returnsValue;
    }

    private bool CheckStatements(BlockStatement block)
    {
        bool returns = false;
        foreach (Statement statement in block.Statements)
        {
            statement.Accept(this);
            returns |= _statementReturns;
        }

        return returns;
    }

    private bool CheckExpression(Expression expression, SourcePosition? valueRequiredAt = null)
    {
        SourcePosition? previousRequiredAt = _valueRequiredAt;
        _valueRequiredAt = valueRequiredAt;
        try
        {
            expression.Accept(this);
            return _expressionReturnsValue;
        }
        finally
        {
            _valueRequiredAt = previousRequiredAt;
        }
    }

    private bool CheckCall(CallExpression call)
    {
        int count = call.Arguments.Count;
        bool returnsValue;
        if (call.Callee == "main")
        {
            throw new CompilationException("точку входа main нельзя вызывать явно", call.Position);
        }

        if (BuiltinFunctions.TryGet(call.Callee, out BuiltinFunctionDescriptor descriptor))
        {
            if (!descriptor.AcceptsArgumentCount(count))
            {
                if (descriptor.MaximumArgumentCount is null)
                {
                    string minimum = descriptor.MinimumArgumentCount == 1 ? "один" : descriptor.MinimumArgumentCount.ToString();
                    throw new CompilationException($"функция {call.Callee} ожидает хотя бы {minimum} аргумент", call.Position);
                }

                string expected = descriptor.MaximumArgumentCount == descriptor.MinimumArgumentCount
                    ? descriptor.MinimumArgumentCount.ToString()
                    : $"хотя бы {descriptor.MinimumArgumentCount}";
                throw new CompilationException($"функция {call.Callee} ожидает аргументов: {expected}", call.Position);
            }

            returnsValue = descriptor.ReturnsValue;
        }
        else
        {
            if (!_functions.TryGetValue(call.Callee, out FunctionDeclaration? function))
            {
                throw new CompilationException($"неизвестная функция '{call.Callee}'", call.Position);
            }

            if (count != function.Parameters.Count)
            {
                throw new CompilationException($"функция {call.Callee} ожидает аргументов: {function.Parameters.Count}", call.Position);
            }

            returnsValue = function.ReturnsValue;
        }

        foreach (Expression argument in call.Arguments)
        {
            _ = CheckExpression(argument, argument.Position);
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