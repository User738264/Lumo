namespace Lumo.Ast;

/// <summary>Описания встроенных функций языка.</summary>
public static class BuiltinFunctions
{
    /// <summary>Функция ввода целого числа.</summary>
    public const string ReadInt = "read_int";

    /// <summary>Функция вывода целого числа; значения не возвращает.</summary>
    public const string PrintInt = "print_int";

    public const string Abs = "abs";

    public const string Min = "min";

    public const string Max = "max";

    private static readonly IReadOnlyDictionary<string, BuiltinFunctionDescriptor> Functions =
        new Dictionary<string, BuiltinFunctionDescriptor>(StringComparer.Ordinal)
        {
            [ReadInt] = new(ReadInt, 0, 0, true),
            [PrintInt] = new(PrintInt, 1, 1, false),
            [Abs] = new(Abs, 1, 1, true),
            [Min] = new(Min, 1, null, true),
            [Max] = new(Max, 1, null, true),
        };

    public static bool IsBuiltin(string name) => Functions.ContainsKey(name);

    public static bool TryGet(string name, out BuiltinFunctionDescriptor descriptor)
    {
        return Functions.TryGetValue(name, out descriptor!);
    }
}