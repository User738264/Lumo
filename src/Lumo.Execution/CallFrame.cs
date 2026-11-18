namespace Lumo.Execution;

/// <summary>Локальные переменные одного вызова. Поиск идёт только по его вложенным блокам.</summary>
internal sealed class CallFrame
{
    private readonly Stack<Dictionary<string, long>> _scopes = new();

    public CallFrame()
    {
        EnterBlock();
    }

    public bool HasReturned { get; set; }

    public void EnterBlock() => _scopes.Push(new Dictionary<string, long>(StringComparer.Ordinal));

    public void LeaveBlock() => _scopes.Pop();

    public void Declare(string name, long value) => _scopes.Peek().Add(name, value);

    public long Read(string name) => FindScope(name)[name];

    public void Assign(string name, long value) => FindScope(name)[name] = value;

    private Dictionary<string, long> FindScope(string name)
    {
        foreach (Dictionary<string, long> scope in _scopes)
        {
            if (scope.ContainsKey(name))
            {
                return scope;
            }
        }

        throw new InvalidOperationException($"Переменная '{name}' не объявлена");
    }
}