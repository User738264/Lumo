using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Базовый класс узлов-выражений.</summary>
public abstract class Expression : AstNode
{
    protected Expression(SourcePosition position)
        : base(position)
    {
    }
}