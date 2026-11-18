using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Чтение значения локальной переменной или параметра.</summary>
public sealed class VariableExpression : Expression
{
    public VariableExpression(string name, SourcePosition position)
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