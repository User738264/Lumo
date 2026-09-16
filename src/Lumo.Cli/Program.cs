using System.Diagnostics.CodeAnalysis;

using Lumo.Lexing;

namespace Lumo.Cli;

/// <summary>
///  Точка входа консольной утилиты: печатает список токенов указанного файла.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Точка входа покрывается ручной проверкой, а не модульными тестами.")]
public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Использование: lumo-lexer <файл.lumo>");
            return 1;
        }

        try
        {
            string text = SourceFile.ReadAllText(args[0]);
            Lexer lexer = new(text);
            foreach (Token token in lexer.Tokenize())
            {
                Console.WriteLine(token);
            }

            return 0;
        }
        catch (LexicalErrorException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
        catch (IOException error)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }
}