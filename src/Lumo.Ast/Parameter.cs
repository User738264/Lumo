using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Именованный параметр функции; в этой итерации его тип всегда int.</summary>
public sealed class Parameter : AstNode
{
    public Parameter(string name, SourcePosition position)
        : base(position)
    {
        Name = name;
    }

    public string Name { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}