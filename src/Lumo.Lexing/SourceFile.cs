using System.Text;

namespace Lumo.Lexing;

/// <summary>
///  Загрузка исходного текста программы в память.
/// </summary>
public static class SourceFile
{
    /// <summary>
    ///  Читает файл с исходным кодом целиком в память.
    ///  Файл должен быть в кодировке UTF-8; метка порядка байтов (BOM) отбрасывается.
    /// </summary>
    public static string ReadAllText(string path)
    {
        return File.ReadAllText(path, Encoding.UTF8);
    }
}