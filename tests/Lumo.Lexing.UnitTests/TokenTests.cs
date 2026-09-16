using Xunit;

namespace Lumo.Lexing.UnitTests;

public class TokenTests
{
    [Fact]
    public void Stores_all_parts_of_token()
    {
        Token token = new(TokenType.StringLiteral, "\"a\\nb\"", new SourcePosition(2, 5), 0, "a\nb");

        Assert.Equal(TokenType.StringLiteral, token.Type);
        Assert.Equal("\"a\\nb\"", token.Lexeme);
        Assert.Equal(new SourcePosition(2, 5), token.Position);
        Assert.Equal("a\nb", token.StringValue);
        Assert.Equal(0L, token.IntValue);
    }

    [Fact]
    public void Formats_token_as_position_type_and_lexeme()
    {
        Token token = new(TokenType.Identifier, "counter", new SourcePosition(7, 3));

        string text = token.ToString();

        Assert.Equal("7:3 Identifier 'counter'", text);
    }
}