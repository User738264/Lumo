using Lumo.Lexing.UnitTests.Helpers;

using Xunit;

namespace Lumo.Lexing.UnitTests;

public class SourceFileTests
{
    [Fact]
    public void Can_read_file_into_memory()
    {
        const string contents = "func main() -> int { return 0; }";
        using TempFile file = TempFile.Create(contents);

        string text = SourceFile.ReadAllText(file.Path);

        Assert.Equal(contents, text);
    }

    [Fact]
    public void Skips_byte_order_mark()
    {
        // TempFile записывает файл в UTF-8 с меткой порядка байтов.
        const string contents = "var x: int = 1;";
        using TempFile file = TempFile.Create(contents);

        string text = SourceFile.ReadAllText(file.Path);

        Assert.Equal('v', text[0]);
    }

    [Fact]
    public void Keeps_non_latin_characters()
    {
        const string contents = "print_string(\"привет\");";
        using TempFile file = TempFile.Create(contents);

        string text = SourceFile.ReadAllText(file.Path);

        Assert.Equal(contents, text);
    }

    [Fact]
    public void Can_tokenize_text_loaded_from_file()
    {
        const string contents = "var x: int = 42;";
        using TempFile file = TempFile.Create(contents);

        IReadOnlyList<TokenType> actualTypes = LexerRunner.TokenTypes(SourceFile.ReadAllText(file.Path));

        TokenType[] expectedTypes =
        [
            TokenType.KeywordVar,
            TokenType.Identifier,
            TokenType.Colon,
            TokenType.KeywordInt,
            TokenType.Assign,
            TokenType.IntLiteral,
            TokenType.Semicolon,
        ];
        Assert.Equal(expectedTypes, actualTypes);
    }
}