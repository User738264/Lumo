using Lumo.Lexing.UnitTests.Helpers;
using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerErrorTests
{
    public static TheoryData<string, string> UnexpectedCharacters()
    {
        return new TheoryData<string, string>
        {
            { "@", "недопустимый символ '@'" },
            { "#", "недопустимый символ '#'" },
            { "$", "недопустимый символ '$'" },
            { "~", "недопустимый символ '~'" },
            { "^", "недопустимый символ '^'" },
            { "?", "недопустимый символ '?'" },
            { "\\", "недопустимый символ '\\'" },
            { "'", "недопустимый символ '''" },

            // Неразрывный пробел не входит в список пробельных символов языка.
            { "\u00a0", "недопустимый символ '\u00a0'" },

            // Одиночные символы логических операторов.
            { "&", "одиночный символ '&' не является оператором, используйте '&&'" },
            { "&x", "одиночный символ '&' не является оператором, используйте '&&'" },
            { "|", "одиночный символ '|' не является оператором, используйте '||'" },
            { "|x", "одиночный символ '|' не является оператором, используйте '||'" },
        };
    }

    [Theory]
    [MemberData(nameof(UnexpectedCharacters))]
    public void Rejects_unexpected_character(string source, string expectedDescription)
    {
        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal(expectedDescription, error.Description);
    }

    [Fact]
    public void Reports_position_of_unexpected_character()
    {
        const string source = "var x: int = 1;\nvar y: int = @;";

        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal(new SourcePosition(2, 14), error.Position);
    }

    [Fact]
    public void Includes_position_in_error_message()
    {
        const string source = "@";

        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal("Строка 1, столбец 1: недопустимый символ '@'", error.Message);
    }

    [Fact]
    public void Rejects_null_source_text()
    {
        Assert.Throws<ArgumentNullException>(() => _ = new Lexer(null!));
    }
}