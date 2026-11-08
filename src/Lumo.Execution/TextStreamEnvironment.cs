namespace Lumo.Execution;

/// <summary>Окружение, которое читает ввод из TextReader и пишет вывод в TextWriter.</summary>
public sealed class TextStreamEnvironment : IEnvironment
{
    // Поток, из которого программа читает ввод.
    private readonly TextReader _input;

    // Поток, в который программа пишет вывод.
    private readonly TextWriter _output;

    public TextStreamEnvironment(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
    }

    public string? ReadLine()
    {
        return _input.ReadLine();
    }

    public void Write(string text)
    {
        _output.Write(text);
    }
}