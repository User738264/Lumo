namespace Lumo.Ast;

/// <summary>Корень дерева: программа целиком (правило program).</summary>
public sealed class ProgramNode : AstNode
{
    public ProgramNode(FunctionDeclaration mainFunction)
        : base(mainFunction.Position)
    {
        MainFunction = mainFunction;
    }

    /// <summary>Объявление функции main — точки входа программы.</summary>
    public FunctionDeclaration MainFunction { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}