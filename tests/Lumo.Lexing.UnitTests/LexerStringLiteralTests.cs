using Lumo.Lexing.UnitTests.Helpers;
using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerStringLiteralTests
{
    public static TheoryData<string, string> ValidLiterals()
    {
        return new TheoryData<string, string>
        {
            // Пустой литерал.
            { "\"\"", "" },

            // Обычный текст.
            { "\"hello\"", "hello" },
            { "\"hello, world\"", "hello, world" },

            // Escape-последовательность перевода строки.
            { "\"a\\nb\"", "a\nb" },

            // Escape-последовательность табуляции.
            { "\"a\\tb\"", "a\tb" },

            // Экранированная кавычка.
            { "\"say \\\"hi\\\"\"", "say \"hi\"" },

            // Экранированная обратная косая черта.
            { "\"c:\\\\tmp\"", "c:\\tmp" },

            // Обратная косая черта перед буквой n, которая уже экранирована.
            { "\"\\\\n\"", "\\n" },

            // Все escape-последовательности сразу.
            { "\"\\n\\t\\\"\\\\\"", "\n\t\"\\" },

            // Литерал может содержать произвольные символы Unicode.
            { "\"строка\"", "строка" },
            { "\"日本語\"", "日本語" },

            // Символы комментариев внутри литерала не начинают комментарий.
            { "\"// не комментарий\"", "// не комментарий" },
            { "\"/* не комментарий */\"", "/* не комментарий */" },

            // Прочие знаки пунктуации внутри литерала не разбираются как операторы.
            { "\"a + b == c;\"", "a + b == c;" },
        };
    }

    public static TheoryData<string, string> InvalidLiterals()
    {
        return new TheoryData<string, string>
        {
            // Литерал не закрыт до конца файла.
            { "\"abc", "незакрытый строковый литерал" },
            { "\"", "незакрытый строковый литерал" },

            // Литерал не закрыт до конца строки.
            { "\"abc\ndef\"", "незакрытый строковый литерал" },
            { "\"abc\r\ndef\"", "незакрытый строковый литерал" },

            // Обратная косая черта в конце файла либо в конце строки.
            { "\"abc\\", "незакрытый строковый литерал" },
            { "\"abc\\\nx\"", "незакрытый строковый литерал" },

            // Неизвестная escape-последовательность.
            { "\"\\q\"", "неизвестная escape-последовательность" },
            { "\"a\\rb\"", "неизвестная escape-последовательность" },
            { "\"\\0\"", "неизвестная escape-последовательность" },
        };
    }

    [Theory]
    [MemberData(nameof(ValidLiterals))]
    public void Can_read_string_literal(string source, string expectedValue)
    {
        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(TokenType.StringLiteral, token.Type);
        Assert.Equal(expectedValue, token.StringValue);
        Assert.Equal(source, token.Lexeme);
    }

    [Theory]
    [MemberData(nameof(InvalidLiterals))]
    public void Rejects_invalid_string_literal(string source, string expectedDescription)
    {
        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal(expectedDescription, error.Description);
    }

    [Fact]
    public void Can_read_two_literals_in_a_row()
    {
        const string source = "\"a\" \"b\"";

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Equal("a", tokens[0].StringValue);
        Assert.Equal("b", tokens[1].StringValue);
        Assert.Equal(TokenType.EndOfFile, tokens[2].Type);
    }

    [Fact]
    public void Reports_position_of_unterminated_literal()
    {
        // Ошибка указывает на начало литерала, а не на конец строки.
        const string source = "var x: string = \"abc";

        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal(new SourcePosition(1, 17), error.Position);
    }

    [Fact]
    public void Reports_position_of_unknown_escape()
    {
        // Ошибка указывает на обратную косую черту.
        const string source = "\"ab\\q\"";

        LexicalErrorException error = LexerRunner.Error(source);

        Assert.Equal(new SourcePosition(1, 4), error.Position);
    }
}