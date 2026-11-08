using Lumo.Ast;
using Lumo.Lexing;

namespace Lumo.Execution.UnitTests.Helpers;

/// <summary>
///  Короткие конструкторы узлов AST, чтобы собирать деревья для тестов вычислителя вручную,
///  без участия лексера и парсера. Позиция по умолчанию — строка 1, столбец 1.
/// </summary>
public static class AstFactory
{
    // Позиция узла, если тест не задал её явно.
    private static readonly SourcePosition DefaultPosition = new(1, 1);

    public static IntLiteralExpression Literal(long value)
    {
        return new IntLiteralExpression(value, DefaultPosition);
    }

    public static UnaryExpression Negate(Expression operand, SourcePosition? position = null)
    {
        return new UnaryExpression(UnaryOperator.Negate, operand, position ?? DefaultPosition);
    }

    public static BinaryExpression Binary(
        BinaryOperator operation,
        Expression left,
        Expression right,
        SourcePosition? position = null)
    {
        return new BinaryExpression(operation, left, right, position ?? DefaultPosition);
    }

    public static CallExpression ReadInt(SourcePosition? position = null)
    {
        return new CallExpression(BuiltinFunctions.ReadInt, [], position ?? DefaultPosition);
    }

    public static CallExpression PrintInt(Expression argument)
    {
        return new CallExpression(BuiltinFunctions.PrintInt, [argument], DefaultPosition);
    }

    public static CallExpression Call(string callee, params Expression[] arguments)
    {
        return new CallExpression(callee, arguments, DefaultPosition);
    }

    public static ExpressionStatement Exec(Expression expression)
    {
        return new ExpressionStatement(expression, DefaultPosition);
    }

    // Собирает программу из функции main с указанными инструкциями.
    public static ProgramNode ProgramOf(params Statement[] statements)
    {
        BlockStatement body = new(statements, DefaultPosition);
        FunctionDeclaration main = new("main", body, DefaultPosition);
        return new ProgramNode(main);
    }
}