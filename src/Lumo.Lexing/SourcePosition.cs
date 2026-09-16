using System.Globalization;

namespace Lumo.Lexing;

/// <summary>
///  Позиция символа в исходном тексте.
///  Нумерация строк и столбцов начинается с единицы.
/// </summary>
public readonly record struct SourcePosition
{
    public SourcePosition(int line, int column)
    {
        Line = line;
        Column = column;
    }

    /// <summary>
    ///  Номер строки, начиная с единицы.
    /// </summary>
    public int Line { get; }

    /// <summary>
    ///  Номер столбца в строке, начиная с единицы.
    /// </summary>
    public int Column { get; }

    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Line}:{Column}");
    }
}