namespace Lumo.Lexing;

/// <summary>
///  Ошибка лексического анализа: исходный текст содержит недопустимую лексему.
/// </summary>
public sealed class LexicalErrorException : Exception
{
    public LexicalErrorException()
    {
    }

    public LexicalErrorException(string message)
        : base(message)
    {
    }

    public LexicalErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public LexicalErrorException(string description, SourcePosition position)
        : base(position.FormatError(description))
    {
        Description = description;
        Position = position;
    }

    /// <summary>
    ///  Описание ошибки без указания позиции.
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    ///  Позиция символа, на котором обнаружена ошибка.
    /// </summary>
    public SourcePosition Position { get; }
}