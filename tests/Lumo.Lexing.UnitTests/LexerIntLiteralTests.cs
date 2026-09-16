using Lumo.Lexing.UnitTests.Helpers;
using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerIntLiteralTests
{
    public static TheoryData<string, long> ValidLiterals()
    {
        return new TheoryData<string, long>
        {
            { "0", 0L },
            { "1", 1L },
            { "7", 7L },
            { "42", 42L },
            { "1200", 1200L },
            { "1000000", 1000000L },

            // Наибольшее значение типа int.
            { "9223372036854775807", 9223372036854775807L },
        };
    }

    public static TheoryData<string, string> InvalidLiterals()
    {
        return new TheoryData<string, string>
        {
            // Ведущие нули запрещены.
            { "00", "целочисленный литерал не может начинаться с нуля" },
            { "01", "целочисленный литерал не может начинаться с нуля" },
            { "007", "целочисленный литерал не может начинаться с нуля" },

            // Значение не помещается в тип int.
            { "9223372036854775808", "целочисленный литерал выходит за пределы типа int" },
            { "99999999999999999999999", "целочисленный литерал выходит за пределы типа int" },
        };
    }

    [Theory]
    [MemberData(nameof(ValidLiterals))]
    public void Can_read_int_literal(string source, long expectedValue)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(TokenType.IntLiteral, token.Type);
        Assert.Equal(source, token.Lexeme);
        Assert.Equal(expectedValue, token.IntValue);
    }

    [Theory]
    [MemberData(nameof(InvalidLiterals))]
    public void Rejects_invalid_int_literal(string source, string expectedDescription)
    {
        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal(expectedDescription, error.Description);
    }

    [Fact]
    public void Reads_minus_before_literal_as_separate_token()
    {
        // Знак не входит в литерал: он обрабатывается как унарный оператор.
        const string source = "-5";

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Equal(TokenType.Minus, tokens[0].Type);
        Assert.Equal(TokenType.IntLiteral, tokens[1].Type);
        Assert.Equal(5L, tokens[1].IntValue);
    }
}