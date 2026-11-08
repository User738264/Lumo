using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Объявление функции (правило main_function). Позиция узла — позиция слова func.</summary>
public sealed class FunctionDeclaration : Declaration
{
    public FunctionDeclaration(string name, BlockStatement body, SourcePosition position)
        : base(position)
    {
        Name = name;
        Body = body;
    }

    /// <summary>Имя функции.</summary>
    public string Name { get; }

    /// <summary>Тело функции.</summary>
    public BlockStatement Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}