using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Целочисленный литерал.</summary>
public sealed class IntLiteralExpression : Expression
{
    public IntLiteralExpression(long value, SourcePosition position)
        : base(position)
    {
        Value = value;
    }

    /// <summary>Значение литерала.</summary>
    public long Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}