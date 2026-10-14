using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>Ошибка разбора или выполнения программы Lumo.</summary>
public sealed class InterpreterException : Exception
{
    public InterpreterException()
    {
    }

    public InterpreterException(string message)
        : base(message)
    {
    }

    public InterpreterException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public InterpreterException(string description, SourcePosition position)
        : base($"Строка {position.Line}, столбец {position.Column}: {description}")
    {
        Position = position;
    }

    public SourcePosition Position { get; }
}