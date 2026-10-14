namespace Lumo.Parsing.IntegrationTests.Helpers;

/// <summary>
///  Программа приёмочного теста из каталога Programs. Файлы теста имеют общее имя:
///  {name}.lumo — исходный код, {name}.stdin — ввод (необязательный), {name}.stdout — ожидаемый вывод,
///  {name}.stderr — ожидаемое сообщение об ошибке (только для программ, завершающихся с ошибкой).
/// </summary>
public sealed record TestProgram(string SourcePath, string Input, CliResult ExpectedResult)
{
    // Каталог Programs копируется в выходной каталог тестов при сборке.
    private static readonly string ProgramsDirectory = Path.Combine(AppContext.BaseDirectory, "Programs");

    // Возвращает имена всех программ из каталога Programs без расширения.
    public static IEnumerable<string> ListNames()
    {
        foreach (string path in Directory.EnumerateFiles(ProgramsDirectory, "*.lumo"))
        {
            yield return Path.GetFileNameWithoutExtension(path);
        }
    }

    // Загружает путь к программе, её ввод и ожидаемый результат выполнения.
    public static TestProgram Load(string name)
    {
        string basePath = Path.Combine(ProgramsDirectory, name);
        string input = ReadIfExists(basePath + ".stdin") ?? "";
        string output = File.ReadAllText(basePath + ".stdout");
        string? error = ReadIfExists(basePath + ".stderr");
        CliResult expectedResult = error == null
            ? new CliResult(0, output, "")
            : CliResult.Failure(output, error.TrimEnd('\r', '\n'));
        return new TestProgram(basePath + ".lumo", input, expectedResult);
    }

    // Читает файл целиком; если файла нет, возвращает null.
    private static string? ReadIfExists(string path)
    {
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}