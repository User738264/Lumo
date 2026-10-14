namespace Lumo.Parsing.IntegrationTests.Helpers;

/// <summary>
///  Результат запуска консольного интерпретатора: код завершения и содержимое потоков вывода и ошибок.
/// </summary>
public sealed record CliResult(int ExitCode, string Output, string Error)
{
    // Результат неудачного запуска: интерпретатор печатает одно сообщение об ошибке и возвращает код 1.
    public static CliResult Failure(string output, string message)
    {
        return new CliResult(1, output, message + Environment.NewLine);
    }
}