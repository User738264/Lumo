using Lumo.Lexing;
using Lumo.Parsing;

namespace Lumo.Cli;

/// <summary>Консольный интерпретатор Lumo.</summary>
public static class Program
{
    public static int Main(string[] args) => Run(args, Console.In, Console.Out, Console.Error);

    /// <summary>Запускает CLI с подменяемыми потоками для приёмочных тестов.</summary>
    public static int Run(string[] args, TextReader input, TextWriter output, TextWriter error)
    {
        if (args.Length != 1)
        {
            error.WriteLine("Использование: lumo <файл.lumo>");
            return 1;
        }

        try
        {
            string text = SourceFile.ReadAllText(args[0]);
            Interpreter.Run(text, input, output);
            return 0;
        }
        catch (Exception exception) when (exception is LexicalErrorException or InterpreterException
            or IOException or UnauthorizedAccessException or ArgumentException)
        {
            error.WriteLine(exception.Message);
            return 1;
        }
    }
}