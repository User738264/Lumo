using Lumo.Ast;
using Lumo.Lexing;

namespace Lumo.Parsing;

/// <summary>
/// Строит AST рекурсивным спуском. После чтения всего файла проверяет имена и типы:
/// вызов функции может находиться раньше её объявления.
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
        List<FunctionDeclaration> functions = [ParseFunctionDeclaration()];
        while (_current.Type == TokenType.KeywordFunc)
        {
            functions.Add(ParseFunctionDeclaration());
        }

        Expect(TokenType.EndOfFile);
        ProgramNode program = new(functions);
        new SemanticAnalyzer().Check(program);
        return program;
    }

    private FunctionDeclaration ParseFunctionDeclaration()
    {
        Token keyword = Expect(TokenType.KeywordFunc);
        Token name = Expect(TokenType.Identifier);
        Expect(TokenType.LeftParen);
        List<Parameter> parameters = _current.Type == TokenType.RightParen ? [] : ParseParameterList();
        Expect(TokenType.RightParen);
        bool returnsValue = _current.Type == TokenType.Arrow;
        if (returnsValue)
        {
            Advance();
            Expect(TokenType.KeywordInt);
        }

        return new FunctionDeclaration(name.Lexeme, name.Position, parameters, returnsValue, ParseBlock(), keyword.Position);
    }

    private List<Parameter> ParseParameterList()
    {
        List<Parameter> parameters = [ParseParameter()];
        while (_current.Type == TokenType.Comma)
        {
            Advance();
            parameters.Add(ParseParameter());
        }

        return parameters;
    }

    private Parameter ParseParameter()
    {
        Token name = Expect(TokenType.Identifier);
        Expect(TokenType.Colon);
        Expect(TokenType.KeywordInt);
        return new Parameter(name.Lexeme, name.Position);
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

    private Statement ParseStatement()
    {
        return _current.Type switch
        {
            TokenType.LeftBrace => ParseBlock(),
            TokenType.KeywordVar => ParseVariableDeclaration(),
            TokenType.KeywordReturn => ParseReturnStatement(),
            _ => ParseExpressionStatement(),
        };
    }

    private VariableDeclarationStatement ParseVariableDeclaration()
    {
        Advance();
        Token name = Expect(TokenType.Identifier);
        Expect(TokenType.Colon);
        Expect(TokenType.KeywordInt);
        Expect(TokenType.Assign);
        Expression initializer = ParseExpression();
        Expect(TokenType.Semicolon);
        return new VariableDeclarationStatement(name.Lexeme, initializer, name.Position);
    }

    private ReturnStatement ParseReturnStatement()
    {
        Token keyword = Advance();
        Expression? value = _current.Type == TokenType.Semicolon ? null : ParseExpression();
        Expect(TokenType.Semicolon);
        return new ReturnStatement(value, keyword.Position);
    }

    private Statement ParseExpressionStatement()
    {
        SourcePosition start = _current.Position;
        Expression left = ParseExpression();
        if (_current.Type == TokenType.Assign)
        {
            if (left is not VariableExpression variable)
            {
                throw Error("слева от '=' ожидается имя переменной", _current);
            }

            Advance();
            Expression value = ParseExpression();
            Expect(TokenType.Semicolon);
            return new AssignmentStatement(variable.Name, value, variable.Position);
        }

        Expect(TokenType.Semicolon);
        return new ExpressionStatement(left, start);
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
            Expression right = ParseMultiplicativeExpression();
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
            Expression right = ParseUnaryExpression();
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
        return new UnaryExpression(UnaryOperator.Negate, operand, operation.Position);
    }

    // Имя без скобок — переменная; со скобками — вызов функции.
    // Скобки нужны только для порядка разбора, поэтому в дереве для них отдельного узла нет.
    private Expression ParsePrimaryExpression()
    {
        switch (_current.Type)
        {
            case TokenType.IntLiteral:
                Token literal = Advance();
                return new IntLiteralExpression(literal.IntValue, literal.Position);
            case TokenType.Identifier:
                Token name = Advance();
                if (_current.Type == TokenType.LeftParen)
                {
                    return ParseCallSuffix(name);
                }

                return new VariableExpression(name.Lexeme, name.Position);
            case TokenType.LeftParen:
                Advance();
                Expression inner = ParseExpression();
                Expect(TokenType.RightParen);
                return inner;
            default:
                throw Error("ожидается число, имя или выражение в скобках", _current);
        }
    }

    private CallExpression ParseCallSuffix(Token name)
    {
        Expect(TokenType.LeftParen);
        List<Expression> arguments = _current.Type == TokenType.RightParen ? [] : ParseArgumentList();
        Expect(TokenType.RightParen);
        return new CallExpression(name.Lexeme, arguments, name.Position);
    }

    // Разбирает аргументы вызова, разделённые запятыми (правило argument_list).
    private List<Expression> ParseArgumentList()
    {
        List<Expression> arguments = [ParseExpression()];
        while (_current.Type == TokenType.Comma)
        {
            Advance();
            arguments.Add(ParseExpression());
        }

        return arguments;
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
            TokenType.Colon => "':'",
            TokenType.Assign => "'='",
            TokenType.KeywordInt => "'int'",
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