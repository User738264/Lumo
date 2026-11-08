using Xunit;

namespace Lumo.Lexing.UnitTests;

public class SourcePositionTests
{
    [Fact]
    public void Formats_error_message_starting_with_line_and_column()
    {
        SourcePosition position = new(4, 12);

        string message = position.FormatError("недопустимый символ '@'");

        Assert.Equal("Строка 4, столбец 12: недопустимый символ '@'", message);
    }
}