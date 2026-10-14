using Lumo.Parsing.IntegrationTests.Helpers;

using Xunit;

namespace Lumo.Parsing.IntegrationTests;

public class ProgramExecutionTests
{
    public static TheoryData<string, string, string> ValidPrograms()
    {
        return new TheoryData<string, string, string>
        {
            // Пустая функция main и инструкция без вывода.
            { "", "", "" },
            { "1 + 2;", "", "" },

            // Приоритет и левая ассоциативность бинарных операторов.
            { "print_int(10 + 7 * (2 * 3 + 4) + 5 + -5);", "", "80\n" },
            { "print_int(10 - 4 - 3); print_int(24 / 4 / 2);", "", "3\n3\n" },

            // Унарный минус, деление нацело и знак остатка.
            { "print_int(--5); print_int(-7 / 2); print_int(-7 % 2); print_int(7 % -2);", "", "5\n-3\n-1\n1\n" },
            { "print_int((-9223372036854775807 - 1) % -1);", "", "0\n" },

            // Вызов print_int в скобках допустим как отдельная инструкция.
            { "(print_int(1));", "", "1\n" },

            // Комментарии пропускаются.
            { "/* комментарий */ print_int(2 + 3 * 4); // комментарий\n", "", "14\n" },

            // Каждый вызов read_int читает одну строку ввода с любым вариантом конца строки.
            { "print_int(read_int() - read_int());", "10\r3\r", "7\n" },
            { "print_int(read_int()); print_int(read_int());", " \t+007\t \r\n-5", "7\n-5\n" },
            { "print_int(read_int());", "-9223372036854775808", "-9223372036854775808\n" },
        };
    }

    [Theory]
    [MemberData(nameof(ValidPrograms))]
    public void Runs_valid_program(string body, string input, string expectedOutput)
    {
        string source = "func main() {" + body + "}";

        CliResult result = CliRunner.RunProgram(source, input);

        Assert.Equal(new CliResult(0, expectedOutput, ""), result);
    }
}