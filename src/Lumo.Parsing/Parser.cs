using Lumo.Ast;
using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>
/// Рекурсивный спуск: по одному методу на правило грамматики. Парсер строит AST программы
/// и ничего не вычисляет. Ошибки компиляции, которые видны уже по тексту программы
/// (синтаксис, неизвестная функция, число аргументов, использование print_int как значения),
/// обнаруживаются здесь же, поэтому выполнять можно только полностью разобранную программу.
/// </summary>
public sealed class Parser
{
    // Лексер, из которого парсер по одному получает токены.
    private readonly Lexer _lexer;

    // Текущий токен — первый ещё не разобранный.
    private Token _current;

    public Parser(string source)
    {
        _lexer = new Lexer(source);
        _current = _lexer.NextToken();
    }

    public ProgramNode ParseProgram()
    {
        FunctionDeclaration mainFunction = ParseMainFunction();
        Expect(TokenType.EndOfFile);
        return new ProgramNode(mainFunction);
    }

    // Разбирает объявление функции main (правило main_function).
    private FunctionDeclaration ParseMainFunction()
    {
        Token keyword = Expect(TokenType.KeywordFunc);
        Token name = Expect(TokenType.Identifier);
        if (name.Lexeme != "main")
        {
            throw Error("ожидается функция main", name);
        }

        Expect(TokenType.LeftParen);
        Expect(TokenType.RightParen);
        BlockStatement body = ParseBlock();
        return new FunctionDeclaration(name.Lexeme, body, keyword.Position);
    }

    // Разбирает блок инструкций в фигурных скобках (правило block).
    private BlockStatement ParseBlock()
    {
        Token openBrace = Expect(TokenType.LeftBrace);
        List<Statement> statements = [];
        while (_current.Type is not (TokenType.RightBrace or TokenType.EndOfFile))
        {
            statements.Add(ParseStatement());
        }

        Expect(TokenType.RightBrace);
        return new BlockStatement(statements, openBrace.Position);
    }

    // Разбирает инструкцию: выражение и точку с запятой (правило statement).
    private ExpressionStatement ParseStatement()
    {
        SourcePosition start = _current.Position;
        Expression expression = ParseExpression();
        Expect(TokenType.Semicolon);
        return new ExpressionStatement(expression, start);
    }

    // Разбирает выражение (правило expression).
    private Expression ParseExpression() => ParseAdditiveExpression();

    // Разбирает сложение и вычитание (правило additive_expression).
    private Expression ParseAdditiveExpression()
    {
        Expression left = ParseMultiplicativeExpression();
        while (_current.Type is TokenType.Plus or TokenType.Minus)
        {
            Token operation = Advance();
            RequireValue(left, operation);
            Expression right = ParseMultiplicativeExpression();
            RequireValue(right, operation);
            left = new BinaryExpression(ToBinaryOperator(operation.Type), left, right, operation.Position);
        }

        return left;
    }

    // Разбирает умножение, деление и взятие остатка (правило multiplicative_expression).
    private Expression ParseMultiplicativeExpression()
    {
        Expression left = ParseUnaryExpression();
        while (_current.Type is TokenType.Star or TokenType.Slash or TokenType.Percent)
        {
            Token operation = Advance();
            RequireValue(left, operation);
            Expression right = ParseUnaryExpression();
            RequireValue(right, operation);
            left = new BinaryExpression(ToBinaryOperator(operation.Type), left, right, operation.Position);
        }

        return left;
    }

    // Разбирает унарный минус (правило unary_expression).
    private Expression ParseUnaryExpression()
    {
        if (_current.Type != TokenType.Minus)
        {
            return ParsePrimaryExpression();
        }

        Token operation = Advance();
        Expression operand = ParseUnaryExpression();
        RequireValue(operand, operation);
        return new UnaryExpression(UnaryOperator.Negate, operand, operation.Position);
    }

    // Разбирает число, вызов функции или выражение в скобках (правило primary_expression).
    // Скобки нужны только для порядка разбора, поэтому в дереве для них отдельного узла нет.
    private Expression ParsePrimaryExpression()
    {
        switch (_current.Type)
        {
            case TokenType.IntLiteral:
                Token literal = Advance();
                return new IntLiteralExpression(literal.IntValue, literal.Position);
            case TokenType.Identifier:
                return ParseFunctionCall();
            case TokenType.LeftParen:
                Advance();
                Expression inner = ParseExpression();
                Expect(TokenType.RightParen);
                return inner;
            default:
                throw Error("ожидается число, вызов функции или выражение в скобках", _current);
        }
    }

    // Разбирает вызов встроенной функции read_int или print_int (правило function_call).
    private CallExpression ParseFunctionCall()
    {
        Token name = Expect(TokenType.Identifier);
        Expect(TokenType.LeftParen);
        if (name.Lexeme is not (BuiltinFunctions.ReadInt or BuiltinFunctions.PrintInt))
        {
            throw Error($"неизвестная функция '{name.Lexeme}'", name);
        }

        List<Expression> arguments = _current.Type == TokenType.RightParen ? [] : ParseArgumentList();
        Expect(TokenType.RightParen);
        int expectedCount = name.Lexeme == BuiltinFunctions.ReadInt ? 0 : 1;
        if (arguments.Count != expectedCount)
        {
            throw Error($"функция {name.Lexeme} ожидает аргументов: {expectedCount}", name);
        }

        return new CallExpression(name.Lexeme, arguments, name.Position);
    }

    // Разбирает аргументы вызова, разделённые запятыми (правило argument_list).
    private List<Expression> ParseArgumentList()
    {
        List<Expression> arguments = [ParseArgument()];
        while (_current.Type == TokenType.Comma)
        {
            Advance();
            arguments.Add(ParseArgument());
        }

        return arguments;
    }

    // Разбирает аргумент вызова; у аргумента должно быть значение.
    private Expression ParseArgument()
    {
        Token start = _current;
        Expression argument = ParseExpression();
        RequireValue(argument, start);
        return argument;
    }

    // Возвращает бинарный оператор AST для токена арифметической операции.
    private static BinaryOperator ToBinaryOperator(TokenType type)
    {
        return type switch
        {
            TokenType.Plus => BinaryOperator.Add,
            TokenType.Minus => BinaryOperator.Subtract,
            TokenType.Star => BinaryOperator.Multiply,
            TokenType.Slash => BinaryOperator.Divide,
            TokenType.Percent => BinaryOperator.Remainder,
            _ => throw new InvalidOperationException("Неизвестный арифметический оператор"),
        };
    }

    // Проверяет, что у выражения есть значение; у вызова print_int значения нет, и это ошибка.
    private static void RequireValue(Expression expression, Token token)
    {
        if (expression is CallExpression { Callee: BuiltinFunctions.PrintInt })
        {
            throw Error("print_int не возвращает значение", token);
        }
    }

    // Создаёт ошибку с позицией указанного токена.
    private static CompilationException Error(string description, Token token)
    {
        return new CompilationException(description, token.Position);
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