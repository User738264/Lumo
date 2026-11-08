using System.Globalization;

using Lumo.Ast;
using Lumo.Lexing;

namespace Lumo.Execution;

/// <summary>
/// Выполняет программу, обходя AST. Выражения вычисляются стековым методом: каждый узел
/// выражения кладёт своё значение на стек, а операция снимает со стека операнды и кладёт результат.
/// Вызов print_int значения не оставляет. Ввод и вывод выполняются через IEnvironment.
/// Вычислитель рассчитан на дерево, прошедшее проверки парсера.
/// </summary>
public sealed class AstEvaluator : IAstVisitor
{
    // Стек значений вычисляемого выражения.
    private readonly Stack<long> _stack = new();

    // Окружение, через которое программа вводит и выводит данные.
    private readonly IEnvironment _environment;

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
        node.MainFunction.Accept(this);
    }

    public void Visit(FunctionDeclaration node)
    {
        node.Body.Accept(this);
    }

    public void Visit(BlockStatement node)
    {
        foreach (Statement statement in node.Statements)
        {
            statement.Accept(this);
        }
    }

    public void Visit(ExpressionStatement node)
    {
        int depth = _stack.Count;
        node.Expression.Accept(this);

        // Значение инструкции отбрасывается; у вызова print_int значения нет, и стек не меняется.
        if (_stack.Count > depth)
        {
            _ = _stack.Pop();
        }
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
        switch (node.Callee)
        {
            case BuiltinFunctions.ReadInt:
                _stack.Push(ReadInt(node.Position));
                break;
            case BuiltinFunctions.PrintInt:
                node.Arguments[0].Accept(this);
                WriteInt(_stack.Pop());
                break;
            default:
                throw new InvalidOperationException($"Неизвестная функция '{node.Callee}'");
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