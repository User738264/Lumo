using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>Ошибка компиляции: синтаксическая ошибка либо нарушение правил языка до начала выполнения.</summary>
public sealed class CompilationException : Exception
{
    public CompilationException()
    {
    }

    public CompilationException(string message)
        : base(message)
    {
    }

    public CompilationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public CompilationException(string description, SourcePosition position)
        : base(position.FormatError(description))
    {
        Position = position;
    }

    public SourcePosition Position { get; }
}