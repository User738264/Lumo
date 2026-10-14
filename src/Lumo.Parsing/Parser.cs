using System.Globalization;

using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>
/// Рекурсивный спуск без AST. В проверочном проходе вычисления и ввод/вывод не выполняются:
/// результаты операций и read_int заменяются нулями, а отсутствие значения у print_int
/// сохраняется как null.
/// </summary>
public sealed class Parser
{
    // Лексер, из которого парсер по одному получает токены.
    private readonly Lexer _lexer;

    // Поток, из которого программа читает ввод.
    private readonly TextReader _input;

    // Поток, в который программа пишет вывод.
    private readonly TextWriter _output;

    // Признак прохода с выполнением; в проверочном проходе операции не выполняются.
    private readonly bool _execute;

    // Текущий токен — первый ещё не разобранный.
    private Token _current;

    public Parser(string source, TextReader input, TextWriter output, bool execute)
    {
        _lexer = new Lexer(source);
        _input = input;
        _output = output;
        _execute = execute;
        _current = _lexer.NextToken();
    }

    public void ParseProgram()
    {
        ParseMainFunction();
        Expect(TokenType.EndOfFile);
    }

    // Разбирает объявление функции main (правило main_function).
    private void ParseMainFunction()
    {
        Expect(TokenType.KeywordFunc);
        Token name = Expect(TokenType.Identifier);
        if (name.Lexeme != "main")
        {
            throw Error("ожидается функция main", name);
        }

        Expect(TokenType.LeftParen);
        Expect(TokenType.RightParen);
        ParseBlock();
    }

    // Разбирает блок инструкций в фигурных скобках (правило block).
    private void ParseBlock()
    {
        Expect(TokenType.LeftBrace);
        while (_current.Type is not (TokenType.RightBrace or TokenType.EndOfFile))
        {
            ParseStatement();
        }

        Expect(TokenType.RightBrace);
    }

    // Разбирает инструкцию: выражение и точку с запятой (правило statement).
    private void ParseStatement()
    {
        ParseExpression();
        Expect(TokenType.Semicolon);
    }

    // Разбирает выражение (правило expression).
    private long? ParseExpression() => ParseAdditiveExpression();

    // Разбирает сложение и вычитание (правило additive_expression).
    private long? ParseAdditiveExpression()
    {
        long? value = ParseMultiplicativeExpression();
        while (_current.Type is TokenType.Plus or TokenType.Minus)
        {
            Token operation = Advance();
            long left = RequireInt(value, operation);
            long right = RequireInt(ParseMultiplicativeExpression(), operation);
            value = EvaluateBinary(operation, left, right);
        }

        return value;
    }

    // Разбирает умножение, деление и взятие остатка (правило multiplicative_expression).
    private long? ParseMultiplicativeExpression()
    {
        long? value = ParseUnaryExpression();
        while (_current.Type is TokenType.Star or TokenType.Slash or TokenType.Percent)
        {
            Token operation = Advance();
            long left = RequireInt(value, operation);
            long right = RequireInt(ParseUnaryExpression(), operation);
            value = EvaluateBinary(operation, left, right);
        }

        return value;
    }

    // Разбирает унарный минус (правило unary_expression).
    private long? ParseUnaryExpression()
    {
        if (_current.Type != TokenType.Minus)
        {
            return ParsePrimaryExpression();
        }

        Token operation = Advance();
        long value = RequireInt(ParseUnaryExpression(), operation);
        try
        {
            return _execute ? checked(-value) : 0;
        }
        catch (OverflowException)
        {
            throw Error("целочисленное переполнение", operation);
        }
    }

    // Разбирает число, вызов функции или выражение в скобках (правило primary_expression).
    private long? ParsePrimaryExpression()
    {
        switch (_current.Type)
        {
            case TokenType.IntLiteral:
                return Advance().IntValue;
            case TokenType.Identifier:
                return ParseFunctionCall();
            case TokenType.LeftParen:
                Advance();
                long? value = ParseExpression();
                Expect(TokenType.RightParen);
                return value;
            default:
                throw Error("ожидается число, вызов функции или выражение в скобках", _current);
        }
    }

    // Разбирает вызов встроенной функции read_int или print_int (правило function_call).
    private long? ParseFunctionCall()
    {
        Token name = Expect(TokenType.Identifier);
        Expect(TokenType.LeftParen);
        if (name.Lexeme is not ("read_int" or "print_int"))
        {
            throw Error($"неизвестная функция '{name.Lexeme}'", name);
        }

        List<long> arguments = _current.Type == TokenType.RightParen ? [] : ParseArgumentList();
        Expect(TokenType.RightParen);
        int expectedCount = name.Lexeme == "read_int" ? 0 : 1;
        if (arguments.Count != expectedCount)
        {
            throw Error($"функция {name.Lexeme} ожидает аргументов: {expectedCount}", name);
        }

        if (name.Lexeme == "read_int")
        {
            return _execute ? ReadInt(name) : 0;
        }

        if (_execute)
        {
            // Спецификация требует перевод строки LF независимо от операционной системы.
            _output.Write(arguments[0].ToString(CultureInfo.InvariantCulture));
            _output.Write('\n');
        }

        return null;
    }

    // Разбирает аргументы вызова, разделённые запятыми (правило argument_list).
    private List<long> ParseArgumentList()
    {
        List<long> arguments = [ParseArgument()];
        while (_current.Type == TokenType.Comma)
        {
            Advance();
            arguments.Add(ParseArgument());
        }

        return arguments;
    }

    // Разбирает аргумент вызова; у аргумента должно быть значение.
    private long ParseArgument()
    {
        Token start = _current;
        return RequireInt(ParseExpression(), start);
    }

    // Читает очередную строку ввода и возвращает записанное в ней целое число.
    private long ReadInt(Token name)
    {
        string? line = _input.ReadLine();
        if (line == null)
        {
            throw Error("поток ввода исчерпан", name);
        }

        string text = line.Trim(' ', '\t');
        string digits = text.StartsWith('+') || text.StartsWith('-') ? text[1..] : text;
        if (digits.Length == 0 || !digits.All(char.IsAsciiDigit))
        {
            throw Error("ожидается десятичная запись целого числа", name);
        }

        // Формат строки уже проверен, поэтому разбор не удаётся, только если число вне диапазона int.
        if (!long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long value))
        {
            throw Error("ожидается целое число в диапазоне int", name);
        }

        return value;
    }

    // Выполняет бинарную арифметическую операцию; в проверочном проходе возвращает ноль.
    private long EvaluateBinary(Token operation, long left, long right)
    {
        if (!_execute)
        {
            return 0;
        }

        if (operation.Type is (TokenType.Slash or TokenType.Percent) && right == 0)
        {
            throw Error("деление на ноль", operation);
        }

        try
        {
            return operation.Type switch
            {
                TokenType.Plus => checked(left + right),
                TokenType.Minus => checked(left - right),
                TokenType.Star => checked(left * right),
                TokenType.Slash => checked(left / right),

                // При делителе -1 остаток всегда равен 0, а long.MinValue % -1 в .NET вызывает OverflowException.
                TokenType.Percent => right == -1 ? 0 : left % right,
                _ => throw new InvalidOperationException("Неизвестный арифметический оператор"),
            };
        }
        catch (OverflowException)
        {
            throw Error("целочисленное переполнение", operation);
        }
    }

    // Возвращает значение выражения; у вызова print_int значения нет, и это ошибка.
    private static long RequireInt(long? value, Token token)
    {
        return value ?? throw Error("print_int не возвращает значение", token);
    }

    // Создаёт ошибку с позицией указанного токена.
    private static InterpreterException Error(string description, Token token)
    {
        return new InterpreterException(description, token.Position);
    }

    // Описывает ожидаемый токен для сообщения об ошибке.
    private static string DescribeTokenType(TokenType type)
    {
        return type switch
        {
            TokenType.KeywordFunc => "'func'",
            TokenType.Identifier => "идентификатор",
            TokenType.LeftParen => "'('",
            TokenType.RightParen => "')'",
            TokenType.LeftBrace => "'{'",
            TokenType.RightBrace => "'}'",
            TokenType.Semicolon => "';'",
            TokenType.EndOfFile => "конец файла",
            _ => throw new InvalidOperationException("Нет описания для категории токена"),
        };
    }

    // Описывает встреченный токен для сообщения об ошибке.
    private static string DescribeToken(Token token)
    {
        return token.Type == TokenType.EndOfFile ? "конец файла" : $"'{token.Lexeme}'";
    }

    // Проверяет категорию текущего токена и переходит к следующему.
    private Token Expect(TokenType type)
    {
        if (_current.Type != type)
        {
            throw Error($"ожидается {DescribeTokenType(type)}, получено {DescribeToken(_current)}", _current);
        }

        return Advance();
    }

    // Переходит к следующему токену и возвращает текущий.
    private Token Advance()
    {
        Token previous = _current;
        _current = _lexer.NextToken();
        return previous;
    }
}