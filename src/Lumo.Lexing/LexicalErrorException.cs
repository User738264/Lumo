using System.Globalization;

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
        : base(FormatMessage(description, position))
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

    // Собирает текст сообщения об ошибке, начинающийся с позиции в исходном тексте.
    private static string FormatMessage(string description, SourcePosition position)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"Строка {position.Line}, столбец {position.Column}: {description}");
    }
}