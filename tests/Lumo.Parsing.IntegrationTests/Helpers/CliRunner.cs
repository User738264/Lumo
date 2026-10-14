using System.Globalization;

using Lumo.Cli;

namespace Lumo.Parsing.IntegrationTests.Helpers;

/// <summary>
///  Вспомогательные методы для запуска консольного интерпретатора с подменёнными потоками.
/// </summary>
public static class CliRunner
{
    // Сохраняет исходный код во временный файл и выполняет его, подавая текст на стандартный ввод.
    public static CliResult RunProgram(string source, string input)
    {
        string path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, source);
            return RunCommand([path], input);
        }
        finally
        {
            File.Delete(path);
        }
    }

    // Запускает интерпретатор с указанными аргументами командной строки, подавая текст на стандартный ввод.
    public static CliResult RunCommand(string[] args, string input)
    {
        using StringReader stdin = new(input);
        using StringWriter stdout = new(CultureInfo.InvariantCulture);
        using StringWriter stderr = new(CultureInfo.InvariantCulture);
        int exitCode = Program.Run(args, stdin, stdout, stderr);
        return new CliResult(exitCode, stdout.ToString(), stderr.ToString());
    }
}