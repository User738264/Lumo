using Lumo.Lexing.UnitTests.Helpers;
using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerTriviaTests
{
    public static TheoryData<string> WhitespaceCases()
    {
        return new TheoryData<string>
        {
            // Пробел.
            " x ",

            // Горизонтальная табуляция.
            "\tx\t",

            // Перевод строки.
            "\nx\n",

            // Возврат каретки без перевода строки.
            "\rx\r",

            // Пара «возврат каретки» и «перевод строки».
            "\r\nx\r\n",

            // Сочетание всех пробельных символов.
            " \t\r\n x \n\r\t ",
        };
    }

    public static TheoryData<string> CommentCases()
    {
        return new TheoryData<string>
        {
            // Строчный комментарий перед токеном и после него.
            "// комментарий\nx\n// комментарий",

            // Строчный комментарий без завершающего перевода строки.
            "x // комментарий",

            // Пустой строчный комментарий.
            "//\nx",

            // Блочный комментарий в одной строке.
            "/* комментарий */ x",

            // Блочный комментарий внутри строки кода.
            "/**/x/**/",

            // Многострочный блочный комментарий.
            "/*\n комментарий\n*/\nx",

            // Блочный комментарий, содержащий символы «/» и «*».
            "/* a / b * c */ x",

            // Вложенности нет: комментарий закрывает первая последовательность «*/».
            "/* /* */ x",
        };
    }

    [Theory]
    [MemberData(nameof(WhitespaceCases))]
    public void Skips_whitespace(string source)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal("x", token.Lexeme);
    }

    [Theory]
    [MemberData(nameof(CommentCases))]
    public void Skips_comments(string source)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal("x", token.Lexeme);
    }

    [Fact]
    public void Returns_only_end_of_file_for_empty_text()
    {
        const string source = "";

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfFile, tokens[0].Type);
    }

    [Fact]
    public void Returns_only_end_of_file_for_whitespace_and_comments()
    {
        const string source = "  \n\t// комментарий\n/* комментарий */\n  ";

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfFile, tokens[0].Type);
    }

    [Fact]
    public void Keeps_token_sequence_around_comments()
    {
        const string source = "a /* 1 */ + // 2\nb";

        IReadOnlyList<TokenType> actualTypes = LexerRunner.TokenTypes(source);

        TokenType[] expectedTypes = [TokenType.Identifier, TokenType.Plus, TokenType.Identifier];
        Assert.Equal(expectedTypes, actualTypes);
    }

    [Fact]
    public void Separates_tokens_by_comment_without_whitespace()
    {
        // Комментарий разделяет токены так же, как пробел.
        const string source = "a/**/b";

        IReadOnlyList<TokenType> actualTypes = LexerRunner.TokenTypes(source);

        TokenType[] expectedTypes = [TokenType.Identifier, TokenType.Identifier];
        Assert.Equal(expectedTypes, actualTypes);
    }

    [Fact]
    public void Rejects_unterminated_block_comment()
    {
        const string source = "x /* комментарий";

        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal("незакрытый блочный комментарий", error.Description);
        Assert.Equal(new SourcePosition(1, 3), error.Position);
    }

    [Fact]
    public void Rejects_block_comment_closed_by_single_star()
    {
        // Последовательности «*» и «/» должны идти подряд.
        const string source = "/* * / ";

        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal("незакрытый блочный комментарий", error.Description);
    }
}