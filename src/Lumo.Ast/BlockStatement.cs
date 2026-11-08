using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Блок инструкций в фигурных скобках (правило block). Позиция узла — позиция «{».</summary>
public sealed class BlockStatement : Statement
{
    public BlockStatement(IReadOnlyList<Statement> statements, SourcePosition position)
        : base(position)
    {
        Statements = statements;
    }

    /// <summary>Инструкции блока в порядке выполнения.</summary>
    public IReadOnlyList<Statement> Statements { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}