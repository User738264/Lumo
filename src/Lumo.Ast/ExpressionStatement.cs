using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>
/// Инструкция-выражение (правило statement): значение выражения, если оно есть, отбрасывается.
/// Позиция узла — позиция первого токена инструкции.
/// </summary>
public sealed class ExpressionStatement : Statement
{
    public ExpressionStatement(Expression expression, SourcePosition position)
        : base(position)
    {
        Expression = expression;
    }

    /// <summary>Вычисляемое выражение.</summary>
    public Expression Expression { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}