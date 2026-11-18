using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Запись вычисленного числа в уже объявленную переменную.</summary>
public sealed class AssignmentStatement : Statement
{
    public AssignmentStatement(string name, Expression value, SourcePosition position)
        : base(position)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }

    public Expression Value { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}