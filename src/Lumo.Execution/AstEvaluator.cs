using System.Globalization;

using Lumo.Ast;
using Lumo.Lexing;

namespace Lumo.Execution;

/// <summary>
/// Выполняет программу, обходя AST. Выражения вычисляются стековым методом: каждый узел
/// выражения кладёт своё значение на стек, а операция снимает со стека операнды и кладёт результат.
/// Каждое выражение, возвращающее значение, оставляет ровно одно значение; семантический анализатор
/// гарантирует этот контракт. Инструкция-выражение отбрасывает это значение, если оно появилось.
/// Процедура не оставляет значения. Visit(FunctionDeclaration) выполняет тело текущего вызова,
/// а Visit(ProgramNode) запускает main, а не обходит все объявления как обычный visitor.
/// Ввод и вывод выполняются через IEnvironment. Вычислитель рассчитан на дерево,
/// прошедшее семантическую проверку.
/// </summary>
public sealed class AstEvaluator : IAstVisitor
{
    private const int MaxCallDepth = 128;

    // Стек значений вычисляемого выражения.
    private readonly Stack<long> _stack = new();

    // Окружение, через которое программа вводит и выводит данные.
    private readonly IEnvironment _environment;

    private Dictionary<string, FunctionDeclaration> _functions = new(StringComparer.Ordinal);
    private CallFrame _frame = new();
    private int _callDepth;

    public AstEvaluator(IEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>Выполняет программу целиком.</summary>
    public void Execute(ProgramNode program)
    {
        program.Accept(this);
    }

    public void Visit(ProgramNode node)
    {
        _stack.Clear();
        _functions = node.Functions.ToDictionary(function => function.Name, StringComparer.Ordinal);
        FunctionDeclaration main = _functions["main"];
        CallFunction(main, [], main.NamePosition);
    }

    public void Visit(FunctionDeclaration node)
    {
        ExecuteStatements(node.Body);
    }

    public void Visit(Parameter node)
    {
    }

    public void Visit(BlockStatement node)
    {
        _frame.EnterBlock();
        try
        {
            ExecuteStatements(node);
        }
        finally
        {
            _frame.LeaveBlock();
        }
    }

    public void Visit(ExpressionStatement node)
    {
        int depth = _stack.Count;
        node.Expression.Accept(this);

        // Семантический анализ гарантирует не более одного результата; выражение-процедура его не оставляет.
        if (_stack.Count > depth)
        {
            _ = _stack.Pop();
        }
    }

    public void Visit(VariableDeclarationStatement node)
    {
        node.Initializer.Accept(this);
        _frame.Declare(node.Name, _stack.Pop());
    }

    public void Visit(AssignmentStatement node)
    {
        node.Value.Accept(this);
        _frame.Assign(node.Name, _stack.Pop());
    }

    public void Visit(ReturnStatement node)
    {
        // Результат остаётся на стеке для выражения в вызывающей функции.
        node.Value?.Accept(this);
        _frame.HasReturned = true;
    }

    public void Visit(VariableExpression node)
    {
        _stack.Push(_frame.Read(node.Name));
    }

    public void Visit(IntLiteralExpression node)
    {
        _stack.Push(node.Value);
    }

    public void Visit(UnaryExpression node)
    {
        node.Operand.Accept(this);
        long operand = _stack.Pop();
        _stack.Push(EvaluateUnary(node, operand));
    }

    public void Visit(BinaryExpression node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
        long right = _stack.Pop();
        long left = _stack.Pop();
        _stack.Push(EvaluateBinary(node, left, right));
    }

    public void Visit(CallExpression node)
    {
        // Вычисляем аргументы в кадре вызывающей функции, строго слева направо.
        long[] arguments = new long[node.Arguments.Count];
        for (int i = 0; i < arguments.Length; ++i)
        {
            node.Arguments[i].Accept(this);
            arguments[i] = _stack.Pop();
        }

        if (BuiltinFunctions.TryGet(node.Callee, out BuiltinFunctionDescriptor builtin))
        {
            switch (builtin.Name)
            {
                case BuiltinFunctions.ReadInt:
                    _stack.Push(ReadInt(node.Position));
                    break;
                case BuiltinFunctions.PrintInt:
                    WriteInt(arguments[0]);
                    break;
                case BuiltinFunctions.Abs:
                    if (arguments[0] == long.MinValue)
                    {
                        throw Error("целочисленное переполнение", node.Position);
                    }

                    _stack.Push(Math.Abs(arguments[0]));
                    break;
                case BuiltinFunctions.Min:
                    _stack.Push(arguments.Min());
                    break;
                case BuiltinFunctions.Max:
                    _stack.Push(arguments.Max());
                    break;
                default:
                    throw new InvalidOperationException($"Вычислитель не поддерживает встроенную функцию '{builtin.Name}'");
            }
        }
        else if (_functions.TryGetValue(node.Callee, out FunctionDeclaration? function))
        {
            CallFunction(function, arguments, node.Position);
        }
        else
        {
            throw new InvalidOperationException($"Неизвестная функция '{node.Callee}'");
        }
    }

    private void ExecuteStatements(BlockStatement block)
    {
        foreach (Statement statement in block.Statements)
        {
            statement.Accept(this);
            if (_frame.HasReturned)
            {
                break;
            }
        }
    }

    private void CallFunction(FunctionDeclaration function, long[] arguments, SourcePosition position)
    {
        if (_callDepth == MaxCallDepth)
        {
            throw Error("превышена глубина вызовов функций", position);
        }

        CallFrame caller = _frame;
        _frame = new CallFrame();
        ++_callDepth;
        try
        {
            for (int i = 0; i < arguments.Length; ++i)
            {
                _frame.Declare(function.Parameters[i].Name, arguments[i]);
            }

            function.Accept(this);
        }
        finally
        {
            --_callDepth;
            _frame = caller;
        }
    }

    // Создаёт ошибку выполнения с указанной позицией.
    private static RuntimeErrorException Error(string description, SourcePosition position)
    {
        return new RuntimeErrorException(description, position);
    }

    // Выполняет унарную операцию.
    private static long EvaluateUnary(UnaryExpression node, long operand)
    {
        try
        {
            return node.Operator switch
            {
                UnaryOperator.Negate => checked(-operand),
                _ => throw new InvalidOperationException("Неизвестный унарный оператор"),
            };
        }
        catch (OverflowException)
        {
            throw Error("целочисленное переполнение", node.Position);
        }
    }

    // Выполняет бинарную арифметическую операцию.
    private static long EvaluateBinary(BinaryExpression node, long left, long right)
    {
        if (node.Operator is (BinaryOperator.Divide or BinaryOperator.Remainder) && right == 0)
        {
            throw Error("деление на ноль", node.Position);
        }

        try
        {
            return node.Operator switch
            {
                BinaryOperator.Add => checked(left + right),
                BinaryOperator.Subtract => checked(left - right),
                BinaryOperator.Multiply => checked(left * right),
                BinaryOperator.Divide => checked(left / right),

                // При делителе -1 остаток всегда равен 0, а long.MinValue % -1 в .NET вызывает OverflowException.
                BinaryOperator.Remainder => right == -1 ? 0 : left % right,
                _ => throw new InvalidOperationException("Неизвестный арифметический оператор"),
            };
        }
        catch (OverflowException)
        {
            throw Error("целочисленное переполнение", node.Position);
        }
    }

    // Читает очередную строку ввода и возвращает записанное в ней целое число.
    private long ReadInt(SourcePosition position)
    {
        string? line = _environment.ReadLine();
        if (line == null)
        {
            throw Error("поток ввода исчерпан", position);
        }

        string text = line.Trim(' ', '\t');
        string digits = text.StartsWith('+') || text.StartsWith('-') ? text[1..] : text;
        if (digits.Length == 0 || !digits.All(char.IsAsciiDigit))
        {
            throw Error("ожидается десятичная запись целого числа", position);
        }

        // Формат строки уже проверен, поэтому разбор не удаётся, только если число вне диапазона int.
        if (!long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long value))
        {
            throw Error("ожидается целое число в диапазоне int", position);
        }

        return value;
    }

    // Выводит число и перевод строки.
    private void WriteInt(long value)
    {
        // Спецификация требует перевод строки LF независимо от операционной системы.
        _environment.Write(string.Create(CultureInfo.InvariantCulture, $"{value}\n"));
    }
}