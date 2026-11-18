using System.Globalization;
using System.Text;

using Lumo.Ast;

namespace Lumo.Parsing.UnitTests.Helpers;

/// <summary>
///  Посетитель, печатающий AST в виде скобочной записи, например: (+ 1 (* 2 3)).
///  Нужен тестам, чтобы сравнивать форму дерева строкой.
/// </summary>
public sealed class AstPrinter : IAstVisitor
{
    // Накопленный текст.
    private readonly StringBuilder _builder = new();

    // Печатает поддерево с корнем в указанном узле.
    public static string Print(AstNode node)
    {
        AstPrinter printer = new();
        node.Accept(printer);
        return printer._builder.ToString();
    }

    public void Visit(Parameter node)
    {
        _builder.Append(node.Name);
    }

    public void Visit(ProgramNode node)
    {
        for (int i = 0; i < node.Functions.Count; ++i)
        {
            if (i > 0)
            {
                _builder.Append(' ');
            }

            node.Functions[i].Accept(this);
        }
    }

    public void Visit(FunctionDeclaration node)
    {
        _builder.Append("func ").Append(node.Name);
        if (node.Parameters.Count != 0)
        {
            _builder.Append('(').AppendJoin(", ", node.Parameters.Select(parameter => parameter.Name)).Append(')');
        }

        if (node.ReturnsValue)
        {
            _builder.Append(" -> int");
        }

        _builder.Append(' ');
        node.Body.Accept(this);
    }

    public void Visit(BlockStatement node)
    {
        _builder.Append('{');
        foreach (Statement statement in node.Statements)
        {
            _builder.Append(' ');
            statement.Accept(this);
        }

        _builder.Append(" }");
    }

    public void Visit(ExpressionStatement node)
    {
        node.Expression.Accept(this);
        _builder.Append(';');
    }

    public void Visit(IntLiteralExpression node)
    {
        _builder.Append(CultureInfo.InvariantCulture, $"{node.Value}");
    }

    public void Visit(VariableExpression node)
    {
        _builder.Append(node.Name);
    }

    public void Visit(VariableDeclarationStatement node)
    {
        _builder.Append("var ").Append(node.Name).Append(" = ");
        node.Initializer.Accept(this);
        _builder.Append(';');
    }

    public void Visit(AssignmentStatement node)
    {
        _builder.Append(node.Name).Append(" = ");
        node.Value.Accept(this);
        _builder.Append(';');
    }

    public void Visit(ReturnStatement node)
    {
        _builder.Append("return");
        if (node.Value != null)
        {
            _builder.Append(' ');
            node.Value.Accept(this);
        }

        _builder.Append(';');
    }

    public void Visit(UnaryExpression node)
    {
        _builder.Append("(neg ");
        node.Operand.Accept(this);
        _builder.Append(')');
    }

    public void Visit(BinaryExpression node)
    {
        _builder.Append('(').Append(OperatorSymbol(node.Operator)).Append(' ');
        node.Left.Accept(this);
        _builder.Append(' ');
        node.Right.Accept(this);
        _builder.Append(')');
    }

    public void Visit(CallExpression node)
    {
        _builder.Append("(call ").Append(node.Callee);
        foreach (Expression argument in node.Arguments)
        {
            _builder.Append(' ');
            argument.Accept(this);
        }

        _builder.Append(')');
    }

    // Возвращает знак бинарного оператора.
    private static string OperatorSymbol(BinaryOperator operation)
    {
        return operation switch
        {
            BinaryOperator.Add => "+",
            BinaryOperator.Subtract => "-",
            BinaryOperator.Multiply => "*",
            BinaryOperator.Divide => "/",
            BinaryOperator.Remainder => "%",
            _ => throw new InvalidOperationException("Неизвестный бинарный оператор"),
        };
    }
}