using System.Globalization;

namespace Lumo.Lexing;

/// <summary>
///  Токен — минимальная лексическая единица, выделенная из исходного текста.
/// </summary>
public sealed record Token
{
    public Token(
        TokenType type,
        string lexeme,
        SourcePosition position,
        long intValue = 0,
        string stringValue = "")
    {
        Type = type;
        Lexeme = lexeme;
        Position = position;
        IntValue = intValue;
        StringValue = stringValue;
    }

    /// <summary>
    ///  Категория токена.
    /// </summary>
    public TokenType Type { get; }

    /// <summary>
    ///  Текст токена ровно в том виде, в каком он записан в исходном тексте.
    ///  Для строкового литерала включает кавычки и необработанные escape-последовательности.
    /// </summary>
    public string Lexeme { get; }

    /// <summary>
    ///  Позиция первого символа токена.
    /// </summary>
    public SourcePosition Position { get; }

    /// <summary>
    ///  Значение целочисленного литерала; для прочих токенов равно нулю.
    /// </summary>
    public long IntValue { get; }

    /// <summary>
    ///  Значение строкового литерала после обработки escape-последовательностей;
    ///  для прочих токенов — пустая строка.
    /// </summary>
    public string StringValue { get; }

    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Position} {Type} '{Lexeme}'");
    }
}