using Lumo.Lexing.UnitTests.Helpers;

using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerPositionTests
{
    public static TheoryData<string, int, int> LineBreakCases()
    {
        return new TheoryData<string, int, int>
        {
            // Перевод строки.
            { "a\nb", 2, 1 },

            // Возврат каретки и перевод строки считаются одним концом строки.
            { "a\r\nb", 2, 1 },

            // Одиночный возврат каретки тоже завершает строку.
            { "a\rb", 2, 1 },

            // Две пустых строки подряд.
            { "a\n\n b", 3, 2 },

            // Конец строки внутри блочного комментария.
            { "a /*\n*/ b", 2, 4 },

            // Конец строки после строчного комментария.
            { "a // комментарий\nb", 2, 1 },
        };
    }

    [Theory]
    [MemberData(nameof(LineBreakCases))]
    public void Tracks_position_after_line_break(string source, int expectedLine, int expectedColumn)
    {
        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Equal(new SourcePosition(expectedLine, expectedColumn), tokens[1].Position);
    }

    [Fact]
    public void Tracks_position_of_every_token()
    {
        const string source = "a\nbb ccc";

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Equal(new SourcePosition(1, 1), tokens[0].Position);
        Assert.Equal(new SourcePosition(2, 1), tokens[1].Position);
        Assert.Equal(new SourcePosition(2, 4), tokens[2].Position);
        Assert.Equal(new SourcePosition(2, 7), tokens[3].Position);
    }

    [Fact]
    public void Counts_tab_as_single_column()
    {
        const string source = "\t\tx";

        Token token = LexerRunner.SingleToken(source);

        Assert.Equal(new SourcePosition(1, 3), token.Position);
    }

    [Fact]
    public void Formats_position_as_line_and_column()
    {
        SourcePosition position = new(3, 14);

        string text = position.ToString();

        Assert.Equal("3:14", text);
    }
}