namespace Lumo.Ast;

/// <summary>Имена встроенных функций языка; их знают и парсер, и вычислитель.</summary>
public static class BuiltinFunctions
{
    /// <summary>Функция ввода целого числа.</summary>
    public const string ReadInt = "read_int";

    /// <summary>Функция вывода целого числа; значения не возвращает.</summary>
    public const string PrintInt = "print_int";
}