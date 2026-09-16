using Lumo.Lexing.UnitTests.Helpers;
using Xunit;

namespace Lumo.Lexing.UnitTests;

public class LexerProgramTests
{
    [Fact]
    public void Can_read_minimal_program()
    {
        const string source = """
                              func main() -> int {
                                  var x: int = 1;
                                  return x;
                              }
                              """;

        IReadOnlyList<TokenType> actualTypes = LexerRunner.TokenTypes(source);

        TokenType[] expectedTypes =
        [
            TokenType.KeywordFunc,
            TokenType.Identifier,
            TokenType.LeftParen,
            TokenType.RightParen,
            TokenType.Arrow,
            TokenType.KeywordInt,
            TokenType.LeftBrace,
            TokenType.KeywordVar,
            TokenType.Identifier,
            TokenType.Colon,
            TokenType.KeywordInt,
            TokenType.Assign,
            TokenType.IntLiteral,
            TokenType.Semicolon,
            TokenType.KeywordReturn,
            TokenType.Identifier,
            TokenType.Semicolon,
            TokenType.RightBrace,
        ];
        Assert.Equal(expectedTypes, actualTypes);
    }

    [Fact]
    public void Can_read_program_with_struct_array_and_loop()
    {
        const string source = """
                              // Точка на плоскости.
                              struct Point {
                                  x: int;
                                  y: int;
                              }

                              func main() -> int {
                                  var points: Point[8];
                                  var i: int = 0;
                                  while (i < 8 && !false) {
                                      points[i].x = i * 2;
                                      points[i].y = i % 3;
                                      i = i + 1;
                                  }

                                  print_string("готово\n");
                                  return 0;
                              }
                              """;

        IReadOnlyList<Token> tokens = LexerRunner.Tokenize(source);

        Assert.Equal(TokenType.KeywordStruct, tokens[0].Type);
        Assert.Equal(TokenType.EndOfFile, tokens[^1].Type);
        IEnumerable<Token> declarations = tokens.Where(token => token.Type == TokenType.KeywordVar);
        Assert.Equal(2, declarations.Count());
        Assert.Contains(tokens, token => token.Type == TokenType.StringLiteral && token.StringValue == "готово\n");
        Assert.Contains(tokens, token => token.Type == TokenType.Identifier && token.Lexeme == "main");
    }

    [Fact]
    public void Returns_end_of_file_token_repeatedly()
    {
        Lexer lexer = new("x");

        lexer.NextToken();
        Token first = lexer.NextToken();
        Token second = lexer.NextToken();

        Assert.Equal(TokenType.EndOfFile, first.Type);
        Assert.Equal(TokenType.EndOfFile, second.Type);
        Assert.Equal(string.Empty, second.Lexeme);
    }
}