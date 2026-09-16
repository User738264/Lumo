using System.Collections.Frozen;

namespace Lumo.Lexing;

/// <summary>
///  Таблица ключевых слов языка Lumo.
/// </summary>
public static class Keywords
{
    // Отображение текста ключевого слова в категорию токена.
    private static readonly FrozenDictionary<string, TokenType> Table =
        new Dictionary<string, TokenType>(StringComparer.Ordinal)
        {
            ["var"] = TokenType.KeywordVar,
            ["func"] = TokenType.KeywordFunc,
            ["return"] = TokenType.KeywordReturn,
            ["if"] = TokenType.KeywordIf,
            ["else"] = TokenType.KeywordElse,
            ["while"] = TokenType.KeywordWhile,
            ["break"] = TokenType.KeywordBreak,
            ["continue"] = TokenType.KeywordContinue,
            ["struct"] = TokenType.KeywordStruct,
            ["true"] = TokenType.KeywordTrue,
            ["false"] = TokenType.KeywordFalse,
            ["int"] = TokenType.KeywordInt,
            ["string"] = TokenType.KeywordString,
            ["bool"] = TokenType.KeywordBool,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    /// <summary>
    ///  Проверяет, является ли текст ключевым словом, и возвращает его категорию.
    ///  Сравнение чувствительно к регистру символов.
    /// </summary>
    public static bool TryGetKeyword(string text, out TokenType type)
    {
        return Table.TryGetValue(text, out type);
    }
}