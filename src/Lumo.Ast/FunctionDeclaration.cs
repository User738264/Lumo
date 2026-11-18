using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Объявление функции. Позиция узла — позиция слова func.</summary>
public sealed class FunctionDeclaration : Declaration
{
    public FunctionDeclaration(
        string name,
        SourcePosition namePosition,
        IReadOnlyList<Parameter> parameters,
        bool returnsValue,
        BlockStatement body,
        SourcePosition position)
        : base(position)
    {
        Name = name;
        NamePosition = namePosition;
        Parameters = parameters;
        ReturnsValue = returnsValue;
        Body = body;
    }

    /// <summary>Имя функции.</summary>
    public string Name { get; }

    public SourcePosition NamePosition { get; }

    public IReadOnlyList<Parameter> Parameters { get; }

    public bool ReturnsValue { get; }

    /// <summary>Тело функции.</summary>
    public BlockStatement Body { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}