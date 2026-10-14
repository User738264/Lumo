using Lumo.Parsing.IntegrationTests.Helpers;

using Xunit;

namespace Lumo.Parsing.IntegrationTests;

public class CommandLineTests
{
    public static TheoryData<string> UnreadablePaths()
    {
        return new TheoryData<string>
        {
            // Файл не существует.
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.lumo"),

            // Путь пуст и не может быть открыт.
            "",
        };
    }

    [Fact]
    public void Prints_usage_without_arguments()
    {
        CliResult result = CliRunner.RunCommand([], "");

        Assert.Equal(CliResult.Failure("", "Использование: lumo <файл.lumo>"), result);
    }

    [Theory]
    [MemberData(nameof(UnreadablePaths))]
    public void Reports_unreadable_file(string path)
    {
        CliResult result = CliRunner.RunCommand([path], "");

        // Текст сообщения формирует .NET, поэтому проверяется только его наличие.
        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
    }
}