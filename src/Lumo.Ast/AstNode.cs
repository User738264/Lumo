using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Базовый класс всех узлов абстрактного синтаксического дерева.</summary>
public abstract class AstNode
{
    protected AstNode(SourcePosition position)
    {
        Position = position;
    }

    /// <summary>
    /// Позиция в исходном тексте, по которой сообщается об ошибках в этом узле.
    /// Для операций это позиция знака операции, для вызова — позиция имени функции.
    /// </summary>
    public SourcePosition Position { get; }

    /// <summary>Передаёт узел посетителю, вызывая подходящий метод Visit.</summary>
    public abstract void Accept(IAstVisitor visitor);
}