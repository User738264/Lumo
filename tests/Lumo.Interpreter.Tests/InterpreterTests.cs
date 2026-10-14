using System.Globalization;

using Lumo.Cli;

using Xunit;

namespace Lumo.Interpreter.Tests;

public sealed class InterpreterTests
{
    [Theory]
    [InlineData("", "", "")]
    [InlineData("1 + 2;", "", "")]
    [InlineData("print_int(10 + 7 * (2 * 3 + 4) + 5 + -5);", "", "80\n")]
    [InlineData("print_int(10 - 4 - 3); print_int(24 / 4 / 2);", "", "3\n3\n")]
    [InlineData("print_int(--5); print_int(-7 / 2); print_int(-7 % 2); print_int(7 % -2);", "", "5\n-3\n-1\n1\n")]
    [InlineData("(print_int(1));", "", "1\n")]
    [InlineData("print_int(read_int() - read_int());", "10\r3\r", "7\n")]
    [InlineData("print_int(read_int()); print_int(read_int());", " \t+007\t \r\n-5", "7\n-5\n")]
    [InlineData("print_int(read_int());", "-9223372036854775808", "-9223372036854775808\n")]
    [InlineData("print_int((-9223372036854775807 - 1) % -1);", "", "0\n")]
    [InlineData("/* comment */ print_int(2 + 3 * 4); // comment\n", "", "14\n")]
    public void ValidProgramRuns(string body, string input, string expected)
    {
        (int code, string output, string error) = Run("func main() {" + body + "}", input);
        Assert.Equal(0, code);
        Assert.Equal(expected, output);
        Assert.Empty(error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("func other() {}")]
    [InlineData("func main(x) {}")]
    [InlineData("func main()")]
    [InlineData("func main() {")]
    [InlineData("func main() {} func main() {}")]
    [InlineData("func main() { print_int(1) }")]
    [InlineData("func main() { print_int(1 + * 2); }")]
    [InlineData("func main() { print_int((1 + 2); }")]
    [InlineData("func main() { ; }")]
    [InlineData("func main() { print_int(1); @ }")]
    [InlineData("func main() { print_int(1); unknown(); }")]
    [InlineData("func main() { print_int(1); read_int(2); }")]
    [InlineData("func main() { print_int(1); print_int(); }")]
    [InlineData("func main() { print_int(1); print_int(2, 3); }")]
    [InlineData("func main() { print_int(1); print_int(print_int(2)); }")]
    [InlineData("func main() { print_int(1); print_int(2) + 3; }")]
    [InlineData("func main() { print_int(1); 3 * print_int(2); }")]
    [InlineData("func main() { print_int(1); -print_int(2); }")]
    [InlineData("func main() { print_int(1); true; }")]
    [InlineData("func main() { print_int(1); +2; }")]
    [InlineData("func main() { print_int(1); read_int; }")]
    public void CompilationErrorPreventsAllOutput(string source)
    {
        (int code, string output, string error) = Run(source, "");
        Assert.Equal(1, code);
        Assert.Empty(output);
        Assert.Contains("Строка", error);
    }

    [Theory]
    [InlineData("1 / 0", "")]
    [InlineData("1 % 0", "")]
    [InlineData("9223372036854775807 + 1", "")]
    [InlineData("(-9223372036854775807 - 1) - 1", "")]
    [InlineData("9223372036854775807 * 2", "")]
    [InlineData("-(-9223372036854775807 - 1)", "")]
    [InlineData("(-9223372036854775807 - 1) / -1", "")]
    [InlineData("read_int()", "")]
    [InlineData("read_int()", "- 5")]
    [InlineData("read_int()", "9223372036854775808")]
    [InlineData("read_int()", "\v5")]
    [InlineData("read_int()", "1.5")]
    [InlineData("read_int()", "abc")]
    [InlineData("read_int()", "5\0")]
    [InlineData("read_int()", "+")]
    public void RuntimeErrorKeepsEarlierOutput(string expression, string input)
    {
        (int code, string output, string error) = Run(
            "func main() { print_int(7); " + expression + "; print_int(9); }", input);
        Assert.Equal(1, code);
        Assert.Equal("7\n", output);
        Assert.Contains("Строка", error);
    }

    [Fact]
    public void InvalidProgramDoesNotReadInput()
    {
        using StringReader input = new("123");
        using StringWriter output = new(CultureInfo.InvariantCulture);
        Assert.Throws<Lumo.Parsing.InterpreterException>(() =>
            Lumo.Parsing.Interpreter.Run("func main() { read_int(); unknown(); }", input, output));
        Assert.Equal("123", input.ReadToEnd());
    }

    [Fact]
    public void ReportsSourcePosition()
    {
        (int code, string output, string error) = Run("func main() {\n  1 + ;\n}", "");
        Assert.Equal(1, code);
        Assert.Empty(output);
        Assert.Contains("Строка 2, столбец 7", error);
    }

    [Fact]
    public void MissingArgumentReturnsFailure()
    {
        using StringWriter output = new(CultureInfo.InvariantCulture);
        using StringWriter error = new(CultureInfo.InvariantCulture);
        Assert.Equal(1, Program.Run([], TextReader.Null, output, error));
        Assert.Empty(output.ToString());
        Assert.Contains("Использование", error.ToString());
    }

    [Fact]
    public void MissingFileReturnsFailure()
    {
        using StringWriter error = new(CultureInfo.InvariantCulture);
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.lumo");
        Assert.Equal(1, Program.Run([path], TextReader.Null, TextWriter.Null, error));
        Assert.NotEmpty(error.ToString());
    }

    private static (int Code, string Output, string Error) Run(string source, string stdin)
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, source);
            using StringReader input = new(stdin);
            using StringWriter output = new(CultureInfo.InvariantCulture);
            using StringWriter error = new(CultureInfo.InvariantCulture);
            output.NewLine = "\n";
            int code = Program.Run([path], input, output, error);
            return (code, output.ToString(), error.ToString());
        }
        finally
        {
            File.Delete(path);
        }
    }
}