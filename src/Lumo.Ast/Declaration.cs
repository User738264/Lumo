using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Базовый класс узлов-объявлений.</summary>
public abstract class Declaration : AstNode
{
    protected Declaration(SourcePosition position)
        : base(position)
    {
    }
}