namespace Lumo.Ast;

/// <summary>Бинарные арифметические операторы языка.</summary>
public enum BinaryOperator
{
    /// <summary>Сложение <c>+</c>.</summary>
    Add,

    /// <summary>Вычитание <c>-</c>.</summary>
    Subtract,

    /// <summary>Умножение <c>*</c>.</summary>
    Multiply,

    /// <summary>Деление нацело <c>/</c>.</summary>
    Divide,

    /// <summary>Остаток от деления <c>%</c>.</summary>
    Remainder,
}