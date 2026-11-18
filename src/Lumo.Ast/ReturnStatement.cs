using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Выход из текущего вызова; у процедуры выражение отсутствует.</summary>
public sealed class ReturnStatement : Statement
{
    public ReturnStatement(Expression? value, SourcePosition position)
        : base(position)
    {
        Value = value;
    }

    public Expression? Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}