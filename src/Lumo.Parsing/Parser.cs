using System.Globalization;

using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>
/// Рекурсивный спуск без AST. В проверочном проходе числа заменяются нулями,
/// но отсутствие значения у print_int сохраняется как null.
/// </summary>
public sealed class Parser
{
    private readonly Lexer _lexer;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly bool _execute;
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

    private void ParseBlock()
    {
        Expect(TokenType.LeftBrace);
        while (_current.Type != TokenType.RightBrace)
        {
            if (_current.Type == TokenType.EndOfFile)
            {
                throw Error("ожидается закрывающая фигурная скобка", _current);
            }

            ParseStatement();
        }

        Expect(TokenType.RightBrace);
    }

    private void ParseStatement()
    {
        ParseExpression();
        Expect(TokenType.Semicolon);
    }

    private long? ParseExpression() => ParseAdditiveExpression();

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

    private long? ParseFunctionCall()
    {
        Token name = Expect(TokenType.Identifier);
        if (name.Lexeme is not ("read_int" or "print_int"))
        {
            throw Error($"неизвестная функция '{name.Lexeme}'", name);
        }

        Expect(TokenType.LeftParen);
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
            _output.WriteLine(arguments[0].ToString(CultureInfo.InvariantCulture));
        }

        return null;
    }

    private List<long> ParseArgumentList()
    {
        List<long> arguments = [];
        do
        {
            Token start = _current;
            arguments.Add(RequireInt(ParseExpression(), start));
            if (_current.Type != TokenType.Comma)
            {
                return arguments;
            }

            Advance();
        }
        while (true);
    }

    private long ReadInt(Token name)
    {
        string? line = _input.ReadLine();
        if (line == null)
        {
            throw Error("поток ввода исчерпан", name);
        }

        string text = line.Trim(' ', '\t');
        int digitStart = text.StartsWith('+') || text.StartsWith('-') ? 1 : 0;
        for (int i = digitStart; i < text.Length; ++i)
        {
            if (text[i] < '0' || text[i] > '9')
            {
                throw Error("ожидается десятичная запись целого числа", name);
            }
        }

        // NumberStyles.AllowLeadingSign не допускает прочие пробельные символы.
        if (!long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long value))
        {
            throw Error("ожидается целое число в диапазоне int", name);
        }

        return value;
    }

    private long EvaluateBinary(Token operation, long left, long right)
    {
        if (!_execute)
        {
            return 0;
        }

        if (operation.Type is TokenType.Slash or TokenType.Percent && right == 0)
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

                // Математический остаток равен нулю и помещается в int.
                TokenType.Percent => right == -1 ? 0 : left % right,
                _ => throw new InvalidOperationException("Неизвестный арифметический оператор"),
            };
        }
        catch (OverflowException)
        {
            throw Error("целочисленное переполнение", operation);
        }
    }

    private static long RequireInt(long? value, Token token)
    {
        return value ?? throw Error("print_int не возвращает значение", token);
    }

    private static InterpreterException Error(string description, Token token)
    {
        return new InterpreterException(description, token.Position);
    }

    private Token Expect(TokenType type)
    {
        if (_current.Type != type)
        {
            throw Error($"ожидается {type}, получено {_current.Type}", _current);
        }

        return Advance();
    }

    private Token Advance()
    {
        Token previous = _current;
        _current = _lexer.NextToken();
        return previous;
    }
}