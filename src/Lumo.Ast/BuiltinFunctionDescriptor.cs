namespace Lumo.Ast;

public sealed record BuiltinFunctionDescriptor(
    string Name,
    int MinimumArgumentCount,
    int? MaximumArgumentCount,
    bool ReturnsValue)
{
    public bool AcceptsArgumentCount(int count)
    {
        return count >= MinimumArgumentCount && (MaximumArgumentCount is null || count <= MaximumArgumentCount);
    }
}