using Lumo.Lexing.UnitTests.Helpers;

using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerIdentifierTests
{
    public static TheoryData<string> ValidIdentifiers()
    {
        return new TheoryData<string>
        {
            // Одна буква.
            "x",
            "X",

            // Одно подчёркивание — допустимый идентификатор.
            "_",

            // Идентификатор может начинаться с подчёркивания.
            "_value",
            "__",
            "_1",

            // Цифры допустимы везде, кроме первой позиции.
            "a1",
            "counter42",
            "x0y1",

            // Подчёркивание допустимо в середине и в конце.
            "very_long_name",
            "name_",
            "A_1_b",
        };
    }

    [Theory]
    [MemberData(nameof(ValidIdentifiers))]
    public void Can_read_identifier(string source)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(TokenType.Identifier, token.Type);
        Assert.Equal(source, token.Lexeme);
    }

    [Fact]
    public void Treats_identifiers_as_case_sensitive()
    {
        const string source = "sum Sum SUM";

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Equal("sum", tokens[0].Lexeme);
        Assert.Equal("Sum", tokens[1].Lexeme);
        Assert.Equal("SUM", tokens[2].Lexeme);
    }

    [Fact]
    public void Stops_identifier_on_first_foreign_character()
    {
        const string source = "abc+def";

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Equal("abc", tokens[0].Lexeme);
        Assert.Equal(TokenType.Plus, tokens[1].Type);
        Assert.Equal("def", tokens[2].Lexeme);
    }

    [Fact]
    public void Rejects_non_latin_letters_in_identifier()
    {
        const string source = "сумма";

        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal("недопустимый символ 'с'", error.Description);
    }
}