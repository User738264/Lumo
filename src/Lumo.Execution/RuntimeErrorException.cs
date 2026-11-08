using Lumo.Lexing;

namespace Lumo.Execution;

/// <summary>Ошибка выполнения: программа прервана, а уже выведенный текст остаётся в выводе.</summary>
public sealed class RuntimeErrorException : Exception
{
    public RuntimeErrorException()
    {
    }

    public RuntimeErrorException(string message)
        : base(message)
    {
    }

    public RuntimeErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public RuntimeErrorException(string description, SourcePosition position)
        : base(position.FormatError(description))
    {
        Position = position;
    }

    public SourcePosition Position { get; }
}