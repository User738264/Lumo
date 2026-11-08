using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>
/// Применение бинарного оператора (правила additive_expression и multiplicative_expression).
/// Позиция узла — позиция знака операции.
/// </summary>
public sealed class BinaryExpression : Expression
{
    public BinaryExpression(BinaryOperator operation, Expression left, Expression right, SourcePosition position)
        : base(position)
    {
        Operator = operation;
        Left = left;
        Right = right;
    }

    /// <summary>Применяемый оператор.</summary>
    public BinaryOperator Operator { get; }

    /// <summary>Левый операнд; вычисляется раньше правого.</summary>
    public Expression Left { get; }

    /// <summary>Правый операнд.</summary>
    public Expression Right { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}