using Lumo.Parsing.IntegrationTests.Helpers;

using Xunit;

namespace Lumo.Parsing.IntegrationTests;

public class CommandLineTests
{
    [Fact]
    public void Prints_usage_without_arguments()
    {
        CliResult result = CliRunner.RunCommand([], "");

        Assert.Equal(CliResult.Failure("", "Использование: lumo <файл.lumo>"), result);
    }

    [Fact]
    public void Reports_missing_file()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.lumo");

        CliResult result = CliRunner.RunCommand([path], "");

        // Текст сообщения формирует .NET, поэтому проверяется только его наличие.
        Assert.Equal(1, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.NotEmpty(result.Error);
    }
}