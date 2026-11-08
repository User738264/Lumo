using System.Text;

namespace Lumo.Execution.UnitTests.Helpers;

/// <summary>
///  Тестовый дублёр окружения: отдаёт заранее заданные строки ввода и запоминает весь вывод.
/// </summary>
public sealed class FakeEnvironment : IEnvironment
{
    // Строки ввода, которые ещё не прочитаны.
    private readonly Queue<string> _input;

    // Накопленный вывод программы.
    private readonly StringBuilder _output = new();

    public FakeEnvironment(params string[] inputLines)
    {
        _input = new Queue<string>(inputLines);
    }

    /// <summary>Весь текст, который программа вывела к текущему моменту.</summary>
    public string Output => _output.ToString();

    /// <summary>Сколько строк ввода программа ещё не прочитала.</summary>
    public int UnreadLineCount => _input.Count;

    public string? ReadLine()
    {
        return _input.TryDequeue(out string? line) ? line : null;
    }

    public void Write(string text)
    {
        _output.Append(text);
    }
}