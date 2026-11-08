using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Применение унарного оператора (правило unary_expression). Позиция узла — позиция знака операции.</summary>
public sealed class UnaryExpression : Expression
{
    public UnaryExpression(UnaryOperator operation, Expression operand, SourcePosition position)
        : base(position)
    {
        Operator = operation;
        Operand = operand;
    }

    /// <summary>Применяемый оператор.</summary>
    public UnaryOperator Operator { get; }

    /// <summary>Операнд оператора.</summary>
    public Expression Operand { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}