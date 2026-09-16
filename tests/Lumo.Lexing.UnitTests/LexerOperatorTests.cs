using Lumo.Lexing.UnitTests.Helpers;
using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerOperatorTests
{
    public static TheoryData<string, TokenType> OperatorCases()
    {
        return new TheoryData<string, TokenType>
        {
            { "+", TokenType.Plus },
            { "-", TokenType.Minus },
            { "*", TokenType.Star },
            { "/", TokenType.Slash },
            { "%", TokenType.Percent },
            { "=", TokenType.Assign },
            { "==", TokenType.Equal },
            { "!=", TokenType.NotEqual },
            { "<", TokenType.Less },
            { "<=", TokenType.LessEqual },
            { ">", TokenType.Greater },
            { ">=", TokenType.GreaterEqual },
            { "&&", TokenType.And },
            { "||", TokenType.Or },
            { "!", TokenType.Not },
            { ".", TokenType.Dot },
            { ",", TokenType.Comma },
            { ";", TokenType.Semicolon },
            { ":", TokenType.Colon },
            { "->", TokenType.Arrow },
            { "(", TokenType.LeftParen },
            { ")", TokenType.RightParen },
            { "{", TokenType.LeftBrace },
            { "}", TokenType.RightBrace },
            { "[", TokenType.LeftBracket },
            { "]", TokenType.RightBracket },
        };
    }

    public static TheoryData<string, string> MaximalMunchCases()
    {
        return new TheoryData<string, string>
        {
            // Два символа «равно» образуют один оператор сравнения.
            { "==", "Equal" },

            // Разделённые пробелом символы образуют два оператора присваивания.
            { "= =", "Assign Assign" },

            // Стрелка не распадается на минус и знак «больше».
            { "->", "Arrow" },
            { "- >", "Minus Greater" },

            // Три символа «равно» разбираются как «==» и «=».
            { "===", "Equal Assign" },

            // Четыре символа «И» образуют два логических оператора.
            { "&&&&", "And And" },

            // Оператор «не равно» имеет приоритет над отрицанием.
            { "!=", "NotEqual" },
            { "! =", "Not Assign" },

            // Знак деления не начинает комментарий, если за ним нет «/» или «*».
            { "a/b", "Identifier Slash Identifier" },
        };
    }

    [Theory]
    [MemberData(nameof(OperatorCases))]
    public void Can_read_operator(string source, TokenType expectedType)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(expectedType, token.Type);
        Assert.Equal(source, token.Lexeme);
    }

    [Theory]
    [MemberData(nameof(MaximalMunchCases))]
    public void Applies_maximal_munch_rule(string source, string expectedTypes)
    {
        string actualTypes = LexerRunner.TokenTypeNames(source);

        Assert.Equal(expectedTypes, actualTypes);
    }

    [Fact]
    public void Can_read_operators_without_spaces()
    {
        const string source = "x=(a+b)*c%2;";

        IReadOnlyList<TokenType> actualTypes = LexerRunner.TokenTypes(source);

        TokenType[] expectedTypes =
        [
            TokenType.Identifier,
            TokenType.Assign,
            TokenType.LeftParen,
            TokenType.Identifier,
            TokenType.Plus,
            TokenType.Identifier,
            TokenType.RightParen,
            TokenType.Star,
            TokenType.Identifier,
            TokenType.Percent,
            TokenType.IntLiteral,
            TokenType.Semicolon,
        ];
        Assert.Equal(expectedTypes, actualTypes);
    }
}