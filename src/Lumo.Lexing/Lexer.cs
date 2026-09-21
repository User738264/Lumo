using System.Globalization;
using System.Text;

namespace Lumo.Lexing;

/// <summary>
///  Лексический анализатор языка Lumo.
/// </summary>
/// <remarks>
///  Анализатор хранит исходный текст целиком в памяти и обрабатывает его посимвольно.
///  Состояние анализатора — это позиция чтения, то есть индекс первого необработанного символа.
///  Позиция чтения только увеличивается: возврат назад не используется, а выбор правила
///  выполняется с помощью предпросмотра символов вперёд (метод <c>Peek</c>).
/// </remarks>
public sealed class Lexer
{
    // Исходный текст программы.
    private readonly string _text;

    // Позиция чтения — индекс первого необработанного символа.
    private int _position;

    // Номер текущей строки, начиная с единицы.
    private int _line = 1;

    // Индекс первого символа текущей строки; нужен для вычисления номера столбца.
    private int _lineStart;

    public Lexer(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _text = text;
    }

    // Признак того, что исходный текст обработан полностью.
    private bool IsAtEnd => _position >= _text.Length;

    // Позиция символа, на который указывает позиция чтения.
    private SourcePosition CurrentPosition => new(_line, _position - _lineStart + 1);

    /// <summary>
    ///  Разбивает исходный текст на токены.
    ///  Последним элементом списка всегда является токен <see cref="TokenType.EndOfFile"/>.
    /// </summary>
    /// <exception cref="LexicalErrorException">Исходный текст содержит недопустимую лексему.</exception>
    public IReadOnlyList<Token> Tokenize()
    {
        List<Token> tokens = [];
        while (true)
        {
            Token token = NextToken();
            tokens.Add(token);
            if (token.Type == TokenType.EndOfFile)
            {
                return tokens;
            }
        }
    }

    /// <summary>
    ///  Выделяет очередной токен.
    ///  По достижении конца текста всегда возвращает токен <see cref="TokenType.EndOfFile"/>.
    /// </summary>
    /// <exception cref="LexicalErrorException">Исходный текст содержит недопустимую лексему.</exception>
    public Token NextToken()
    {
        SkipTrivia();

        SourcePosition start = CurrentPosition;
        if (IsAtEnd)
        {
            return new Token(TokenType.EndOfFile, string.Empty, start);
        }

        char ch = Peek();
        if (IsIdentifierStart(ch))
        {
            return ReadIdentifierOrKeyword(start);
        }

        if (IsDigit(ch))
        {
            return ReadIntLiteral(start);
        }

        if (ch == '"')
        {
            return ReadStringLiteral(start);
        }

        return ReadOperator(start);
    }

    // Проверяет, может ли символ начинать идентификатор.
    private static bool IsIdentifierStart(char ch)
    {
        return IsLetter(ch) || ch == '_';
    }

    // Проверяет, может ли символ продолжать идентификатор.
    private static bool IsIdentifierPart(char ch)
    {
        return IsIdentifierStart(ch) || IsDigit(ch);
    }

    // Проверяет, является ли символ латинской буквой.
    private static bool IsLetter(char ch)
    {
        return ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');
    }

    // Проверяет, является ли символ десятичной цифрой.
    private static bool IsDigit(char ch)
    {
        return ch is >= '0' and <= '9';
    }

    // Проверяет, является ли символ пробельным.
    private static bool IsWhitespace(char ch)
    {
        return ch is ' ' or '\t' or '\n' or '\r';
    }

    // Проверяет, завершает ли символ строку текста.
    private static bool IsLineBreak(char ch)
    {
        return ch is '\n' or '\r';
    }

    // Распознаёт двухсимвольный оператор.
    private static TokenType? MatchPairOperator(char first, char second)
    {
        return (first, second) switch
        {
            ('=', '=') => TokenType.Equal,
            ('!', '=') => TokenType.NotEqual,
            ('<', '=') => TokenType.LessEqual,
            ('>', '=') => TokenType.GreaterEqual,
            ('&', '&') => TokenType.And,
            ('|', '|') => TokenType.Or,
            ('-', '>') => TokenType.Arrow,
            _ => null,
        };
    }

    // Распознаёт односимвольный оператор либо знак пунктуации.
    private static TokenType? MatchSingleOperator(char ch)
    {
        return ch switch
        {
            '+' => TokenType.Plus,
            '-' => TokenType.Minus,
            '*' => TokenType.Star,
            '/' => TokenType.Slash,
            '%' => TokenType.Percent,
            '=' => TokenType.Assign,
            '!' => TokenType.Not,
            '<' => TokenType.Less,
            '>' => TokenType.Greater,
            '.' => TokenType.Dot,
            ',' => TokenType.Comma,
            ';' => TokenType.Semicolon,
            ':' => TokenType.Colon,
            '(' => TokenType.LeftParen,
            ')' => TokenType.RightParen,
            '{' => TokenType.LeftBrace,
            '}' => TokenType.RightBrace,
            '[' => TokenType.LeftBracket,
            ']' => TokenType.RightBracket,
            _ => null,
        };
    }

    // Составляет описание символа, который не может начинать ни один токен.
    private static string DescribeUnexpectedCharacter(char ch)
    {
        return ch switch
        {
            '&' => "одиночный символ '&' не является оператором, используйте '&&'",
            '|' => "одиночный символ '|' не является оператором, используйте '||'",
            _ => string.Create(CultureInfo.InvariantCulture, $"недопустимый символ '{ch}'"),
        };
    }

    // Возвращает символ, отстоящий от позиции чтения на указанное число символов,
    // не сдвигая позицию чтения. За концом текста возвращает нулевой символ.
    private char Peek(int offset = 0)
    {
        int index = _position + offset;
        return index < _text.Length ? _text[index] : '\0';
    }

    // Сдвигает позицию чтения на один символ вперёд и возвращает прочитанный символ.
    private char Advance()
    {
        char ch = _text[_position];
        _position++;

        // Конец строки — это либо LF, либо CR, либо пара CR LF;
        // в последнем случае строка считается законченной на символе LF.
        if (ch == '\n' || (ch == '\r' && Peek() != '\n'))
        {
            _line++;
            _lineStart = _position;
        }

        return ch;
    }

    // Пропускает пробельные символы и комментарии, разделяющие токены.
    private void SkipTrivia()
    {
        while (!IsAtEnd)
        {
            char ch = Peek();
            if (IsWhitespace(ch))
            {
                Advance();
            }
            else if (ch == '/' && Peek(1) == '/')
            {
                SkipLineComment();
            }
            else if (ch == '/' && Peek(1) == '*')
            {
                SkipBlockComment();
            }
            else
            {
                return;
            }
        }
    }

    // Пропускает строчный комментарий; завершающий символ конца строки не обрабатывается.
    private void SkipLineComment()
    {
        while (!IsAtEnd && !IsLineBreak(Peek()))
        {
            Advance();
        }
    }

    // Пропускает блочный комментарий. Вложенность не поддерживается:
    // комментарий завершает первая же последовательность "*/".
    private void SkipBlockComment()
    {
        SourcePosition start = CurrentPosition;

        // Пропускаем открывающую последовательность "/*".
        Advance();
        Advance();

        while (!IsAtEnd)
        {
            if (Peek() == '*' && Peek(1) == '/')
            {
                Advance();
                Advance();
                return;
            }

            Advance();
        }

        throw new LexicalErrorException("незакрытый блочный комментарий", start);
    }

    // Читает идентификатор либо ключевое слово.
    private Token ReadIdentifierOrKeyword(SourcePosition start)
    {
        int begin = _position;
        while (!IsAtEnd && IsIdentifierPart(Peek()))
        {
            Advance();
        }

        string lexeme = _text[begin.._position];
        if (Keywords.TryGetKeyword(lexeme, out TokenType keyword))
        {
            return new Token(keyword, lexeme, start);
        }

        return new Token(TokenType.Identifier, lexeme, start);
    }

    // Читает целочисленный литерал.
    private Token ReadIntLiteral(SourcePosition start)
    {
        int begin = _position;
        bool startsWithZero = Peek() == '0';
        while (!IsAtEnd && IsDigit(Peek()))
        {
            Advance();
        }

        // Литерал не может сливаться с последующим идентификатором, например в записи "12abc".
        if (IsIdentifierStart(Peek()))
        {
            throw new LexicalErrorException("после целочисленного литерала не может сразу следовать буква или '_'", start);
        }

        string lexeme = _text[begin.._position];
        if (startsWithZero && lexeme.Length > 1)
        {
            throw new LexicalErrorException("целочисленный литерал не может начинаться с нуля", start);
        }

        if (!long.TryParse(lexeme, NumberStyles.None, CultureInfo.InvariantCulture, out long value))
        {
            throw new LexicalErrorException("целочисленный литерал выходит за пределы типа int", start);
        }

        return new Token(TokenType.IntLiteral, lexeme, start, value);
    }

    // Читает строковый литерал вместе с обработкой escape-последовательностей.
    private Token ReadStringLiteral(SourcePosition start)
    {
        int begin = _position;
        StringBuilder value = new();

        // Пропускаем открывающую кавычку.
        Advance();

        while (true)
        {
            if (IsAtEnd || IsLineBreak(Peek()))
            {
                throw new LexicalErrorException("незакрытый строковый литерал", start);
            }

            SourcePosition characterStart = CurrentPosition;
            char ch = Advance();
            if (ch == '"')
            {
                return new Token(TokenType.StringLiteral, _text[begin.._position], start, 0, value.ToString());
            }

            if (ch == '\\')
            {
                value.Append(ReadEscape(start, characterStart));
            }
            else
            {
                value.Append(ch);
            }
        }
    }

    // Обрабатывает escape-последовательность; обратная косая черта уже прочитана.
    private char ReadEscape(SourcePosition literalStart, SourcePosition escapeStart)
    {
        if (IsAtEnd || IsLineBreak(Peek()))
        {
            throw new LexicalErrorException("незакрытый строковый литерал", literalStart);
        }

        char ch = Advance();
        return ch switch
        {
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            '"' => '"',
            '\\' => '\\',
            _ => throw new LexicalErrorException("неизвестная escape-последовательность", escapeStart),
        };
    }

    // Читает оператор либо знак пунктуации.
    // Согласно принципу максимального захвата сначала проверяются двухсимвольные лексемы.
    private Token ReadOperator(SourcePosition start)
    {
        int begin = _position;
        char first = Peek();

        TokenType? pair = MatchPairOperator(first, Peek(1));
        if (pair.HasValue)
        {
            Advance();
            Advance();
            return new Token(pair.Value, _text[begin.._position], start);
        }

        TokenType? single = MatchSingleOperator(first);
        if (single.HasValue)
        {
            Advance();
            return new Token(single.Value, _text[begin.._position], start);
        }

        throw new LexicalErrorException(DescribeUnexpectedCharacter(first), start);
    }
}