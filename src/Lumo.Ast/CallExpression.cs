using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Вызов функции (правило function_call). Позиция узла — позиция имени функции.</summary>
public sealed class CallExpression : Expression
{
    public CallExpression(string callee, IReadOnlyList<Expression> arguments, SourcePosition position)
        : base(position)
    {
        Callee = callee;
        Arguments = arguments;
    }

    /// <summary>Имя вызываемой функции.</summary>
    public string Callee { get; }

    /// <summary>Аргументы вызова в порядке записи.</summary>
    public IReadOnlyList<Expression> Arguments { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}