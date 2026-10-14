namespace Lumo.Parsing;

/// <summary>Проверяет программу целиком, затем выполняет её без построения AST.</summary>
public static class Interpreter
{
    public static void Run(string source, TextReader input, TextWriter output)
    {
        Parser validation = new(source, TextReader.Null, TextWriter.Null, execute: false);
        validation.ParseProgram();
        Parser execution = new(source, input, output, execute: true);
        execution.ParseProgram();
    }
}