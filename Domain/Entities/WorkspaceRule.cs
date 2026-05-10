namespace WorkTogetherly.Domain.Entities;

public class WorkspaceRule
{
    public int WorkspaceId { get; private set; }
    public int RuleId { get; private set; }

    public Workspace Workspace { get; private set; } = null!;
    public Rule Rule { get; private set; } = null!;

    private WorkspaceRule() { }

    public static WorkspaceRule Create(int workspaceId, int ruleId) =>
        new() { WorkspaceId = workspaceId, RuleId = ruleId };
}
