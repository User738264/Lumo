namespace Lumo.Execution;

/// <summary>Окружение выполняемой программы: единственный источник ввода и приёмник вывода.</summary>
public interface IEnvironment
{
    /// <summary>
    /// Читает очередную строку ввода без символов конца строки.
    /// Возвращает null, если поток ввода исчерпан.
    /// </summary>
    string? ReadLine();

    /// <summary>Записывает текст в поток вывода как есть.</summary>
    void Write(string text);
}