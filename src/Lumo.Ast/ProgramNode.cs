namespace Lumo.Ast;

/// <summary>Корень дерева: программа целиком (правило program).</summary>
public sealed class ProgramNode : AstNode
{
    public ProgramNode(IReadOnlyList<FunctionDeclaration> functions)
        : base(functions[0].Position)
    {
        Functions = functions;
    }

    public IReadOnlyList<FunctionDeclaration> Functions { get; }

    /// <summary>Точка входа проверенной программы.</summary>
    public FunctionDeclaration MainFunction => Functions.First(function => function.Name == "main");

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}