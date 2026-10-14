using Lumo.Parsing.IntegrationTests.Helpers;

using Xunit;

namespace Lumo.Parsing.IntegrationTests;

/// <summary>
///  Приёмочные тесты: выполняют программы из каталога Programs через CLI с подменёнными потоками
///  и сравнивают вывод, сообщение об ошибке и код завершения с ожидаемыми.
/// </summary>
public class AcceptanceTests
{
    public static TheoryData<string> ProgramNames()
    {
        TheoryData<string> names = new();
        foreach (string name in TestProgram.ListNames().Order(StringComparer.Ordinal))
        {
            names.Add(name);
        }

        return names;
    }

    [Theory]
    [MemberData(nameof(ProgramNames))]
    public void Runs_program_from_file(string name)
    {
        TestProgram program = TestProgram.Load(name);

        CliResult result = CliRunner.RunCommand([program.SourcePath], program.Input);

        Assert.Equal(program.ExpectedResult, result);
    }
}