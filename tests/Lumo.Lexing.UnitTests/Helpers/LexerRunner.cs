using Xunit;

namespace Lumo.Lexing.UnitTests.Helpers;

/// <summary>
///  Вспомогательные методы для запуска лексического анализатора в тестах.
/// </summary>
public static class LexerRunner
{
    // Разбивает текст на токены, включая завершающий токен конца файла.
    public static IReadOnlyList<Token> Tokenize(string text)
    {
        Lexer lexer = new(text);
        return lexer.Tokenize();
    }

    // Возвращает категории всех токенов текста, кроме токена конца файла.
    public static IReadOnlyList<TokenType> TokenTypes(string text)
    {
        IReadOnlyList<Token> tokens = Tokenize(text);
        return tokens.Take(tokens.Count - 1).Select(token => token.Type).ToList();
    }

    // Возвращает категории токенов текста одной строкой, разделяя их пробелами.
    public static string TokenTypeNames(string text)
    {
        return string.Join(' ', TokenTypes(text));
    }

    // Проверяет, что текст содержит ровно один токен, и возвращает его.
    public static Token SingleToken(string text)
    {
        IReadOnlyList<Token> tokens = Tokenize(text);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.EndOfFile, tokens[^1].Type);
        return tokens[0];
    }

    // Проверяет, что разбор текста завершается лексической ошибкой, и возвращает её.
    public static LexicalErrorException Error(string text)
    {
        Lexer lexer = new(text);
        return Assert.Throws<LexicalErrorException>(() => _ = lexer.Tokenize());
    }
}