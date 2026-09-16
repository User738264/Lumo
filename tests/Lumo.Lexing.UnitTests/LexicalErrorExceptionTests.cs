using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexicalErrorExceptionTests
{
    [Fact]
    public void Builds_message_from_description_and_position()
    {
        LexicalErrorException error = new("недопустимый символ '@'", new SourcePosition(4, 12));

        Assert.Equal("Строка 4, столбец 12: недопустимый символ '@'", error.Message);
        Assert.Equal("недопустимый символ '@'", error.Description);
        Assert.Equal(new SourcePosition(4, 12), error.Position);
    }
}