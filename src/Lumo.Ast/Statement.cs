using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Базовый класс узлов-инструкций.</summary>
public abstract class Statement : AstNode
{
    protected Statement(SourcePosition position)
        : base(position)
    {
    }
}