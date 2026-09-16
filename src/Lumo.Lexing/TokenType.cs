namespace Lumo.Lexing;

/// <summary>
///  Категории токенов языка Lumo.
/// </summary>
public enum TokenType
{
    /// <summary>Конец исходного текста.</summary>
    EndOfFile,

    /// <summary>Идентификатор.</summary>
    Identifier,

    /// <summary>Целочисленный литерал.</summary>
    IntLiteral,

    /// <summary>Строковый литерал.</summary>
    StringLiteral,

    /// <summary>Ключевое слово <c>var</c>.</summary>
    KeywordVar,

    /// <summary>Ключевое слово <c>func</c>.</summary>
    KeywordFunc,

    /// <summary>Ключевое слово <c>return</c>.</summary>
    KeywordReturn,

    /// <summary>Ключевое слово <c>if</c>.</summary>
    KeywordIf,

    /// <summary>Ключевое слово <c>else</c>.</summary>
    KeywordElse,

    /// <summary>Ключевое слово <c>while</c>.</summary>
    KeywordWhile,

    /// <summary>Ключевое слово <c>break</c>.</summary>
    KeywordBreak,

    /// <summary>Ключевое слово <c>continue</c>.</summary>
    KeywordContinue,

    /// <summary>Ключевое слово <c>struct</c>.</summary>
    KeywordStruct,

    /// <summary>Булев литерал <c>true</c>.</summary>
    KeywordTrue,

    /// <summary>Булев литерал <c>false</c>.</summary>
    KeywordFalse,

    /// <summary>Ключевое слово <c>int</c>.</summary>
    KeywordInt,

    /// <summary>Ключевое слово <c>string</c>.</summary>
    KeywordString,

    /// <summary>Ключевое слово <c>bool</c>.</summary>
    KeywordBool,

    /// <summary>Оператор сложения <c>+</c>.</summary>
    Plus,

    /// <summary>Оператор вычитания либо унарный минус <c>-</c>.</summary>
    Minus,

    /// <summary>Оператор умножения <c>*</c>.</summary>
    Star,

    /// <summary>Оператор деления <c>/</c>.</summary>
    Slash,

    /// <summary>Оператор взятия остатка <c>%</c>.</summary>
    Percent,

    /// <summary>Оператор присваивания <c>=</c>.</summary>
    Assign,

    /// <summary>Оператор сравнения «равно» <c>==</c>.</summary>
    Equal,

    /// <summary>Оператор сравнения «не равно» <c>!=</c>.</summary>
    NotEqual,

    /// <summary>Оператор сравнения «меньше» <c>&lt;</c>.</summary>
    Less,

    /// <summary>Оператор сравнения «меньше или равно» <c>&lt;=</c>.</summary>
    LessEqual,

    /// <summary>Оператор сравнения «больше» <c>&gt;</c>.</summary>
    Greater,

    /// <summary>Оператор сравнения «больше или равно» <c>&gt;=</c>.</summary>
    GreaterEqual,

    /// <summary>Оператор логического «И» <c>&amp;&amp;</c>.</summary>
    And,

    /// <summary>Оператор логического «ИЛИ» <c>||</c>.</summary>
    Or,

    /// <summary>Оператор логического отрицания <c>!</c>.</summary>
    Not,

    /// <summary>Оператор доступа к полю <c>.</c>.</summary>
    Dot,

    /// <summary>Запятая <c>,</c>.</summary>
    Comma,

    /// <summary>Точка с запятой <c>;</c>.</summary>
    Semicolon,

    /// <summary>Двоеточие <c>:</c>.</summary>
    Colon,

    /// <summary>Стрелка <c>-&gt;</c>, обозначающая тип возвращаемого значения.</summary>
    Arrow,

    /// <summary>Открывающая круглая скобка <c>(</c>.</summary>
    LeftParen,

    /// <summary>Закрывающая круглая скобка <c>)</c>.</summary>
    RightParen,

    /// <summary>Открывающая фигурная скобка <c>{</c>.</summary>
    LeftBrace,

    /// <summary>Закрывающая фигурная скобка <c>}</c>.</summary>
    RightBrace,

    /// <summary>Открывающая квадратная скобка <c>[</c>.</summary>
    LeftBracket,

    /// <summary>Закрывающая квадратная скобка <c>]</c>.</summary>
    RightBracket,
}