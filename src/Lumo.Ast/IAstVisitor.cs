namespace Lumo.Ast;

/// <summary>Посетитель узлов AST: по одному методу Visit на каждый вид узла.</summary>
public interface IAstVisitor
{
    void Visit(ProgramNode node);

    void Visit(FunctionDeclaration node);

    void Visit(BlockStatement node);

    void Visit(ExpressionStatement node);

    void Visit(IntLiteralExpression node);

    void Visit(UnaryExpression node);

    void Visit(BinaryExpression node);

    void Visit(CallExpression node);
}