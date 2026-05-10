namespace WorkTogetherly.Domain.Entities;

public class Rule
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<WorkspaceRule> WorkspaceRules { get; private set; } = [];

    private Rule() { }

    public static Rule Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Rule { Name = name };
    }
}
