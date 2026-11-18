using Lumo.Lexing;

namespace Lumo.Ast;

/// <summary>Именованный параметр функции; в этой итерации его тип всегда int.</summary>
public sealed record Parameter(string Name, SourcePosition Position);