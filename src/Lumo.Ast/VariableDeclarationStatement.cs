using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Объявление локальной переменной с обязательным начальным значением.</summary>
public sealed class VariableDeclarationStatement : Statement
{
    public VariableDeclarationStatement(string name, Expression initializer, SourcePosition position)
        : base(position)
    {
        Name = name;
        Initializer = initializer;
    }

    public string Name { get; }

    public Expression Initializer { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}