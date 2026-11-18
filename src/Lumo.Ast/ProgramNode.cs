using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Корень дерева: программа целиком (правило program).</summary>
public sealed class ProgramNode : AstNode
{
    public ProgramNode(IReadOnlyList<FunctionDeclaration> functions)
        : base(functions.Count > 0 ? functions[0].Position : new SourcePosition(1, 1))
    {
        Functions = functions;
    }

    public IReadOnlyList<FunctionDeclaration> Functions { get; }

    public override void Accept(IAstVisitor visitor)
    {
        visitor.Visit(this);
    }
}