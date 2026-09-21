using Lumo.Lexing.UnitTests.Helpers;

using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerKeywordTests
{
    public static TheoryData<string, TokenType> KeywordCases()
    {
        return new TheoryData<string, TokenType>
        {
            { "var", TokenType.KeywordVar },
            { "func", TokenType.KeywordFunc },
            { "return", TokenType.KeywordReturn },
            { "if", TokenType.KeywordIf },
            { "else", TokenType.KeywordElse },
            { "while", TokenType.KeywordWhile },
            { "break", TokenType.KeywordBreak },
            { "continue", TokenType.KeywordContinue },
            { "struct", TokenType.KeywordStruct },
            { "true", TokenType.KeywordTrue },
            { "false", TokenType.KeywordFalse },
            { "int", TokenType.KeywordInt },
            { "string", TokenType.KeywordString },
            { "bool", TokenType.KeywordBool },
        };
    }

    public static TheoryData<string> IdentifierCases()
    {
        return new TheoryData<string>
        {
            // Ключевое слово, записанное в другом регистре, — это идентификатор.
            "Var",
            "IF",
            "True",

            // Ключевое слово как часть более длинного имени — это идентификатор.
            "iffy",
            "ifx",
            "whiles",
            "_if",
            "if_",
            "int32",

            // Имена точки входа и встроенных функций ключевыми словами не являются.
            "main",
            "print_int",
            "print_string",
            "print_bool",
            "read_int",
            "read_string",
        };
    }

    [Theory]
    [MemberData(nameof(KeywordCases))]
    public void Can_read_keyword(string source, TokenType expectedType)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(expectedType, token.Type);
        Assert.Equal(source, token.Lexeme);
    }

    [Theory]
    [MemberData(nameof(IdentifierCases))]
    public void Reads_word_similar_to_keyword_as_identifier(string source)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal(source, token.Lexeme);
    }

    [Fact]
    public void Can_read_keywords_separated_by_punctuation()
    {
        const string source = "if(true){return false;}";

        IReadOnlyList<TokenType> actualTypes = LexerRunner.TokenTypes(source);

        TokenType[] expectedTypes =
        [
            TokenType.KeywordIf,
            TokenType.LeftParen,
            TokenType.KeywordTrue,
            TokenType.RightParen,
            TokenType.LeftBrace,
            TokenType.KeywordReturn,
            TokenType.KeywordFalse,
            TokenType.Semicolon,
            TokenType.RightBrace,
        ];
        Assert.Equal(expectedTypes, actualTypes);
    }
}