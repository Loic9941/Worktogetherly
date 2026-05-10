namespace WorkTogetherly.Domain.Entities;

public class WorkspaceMaterial
{
    public int WorkspaceId { get; private set; }
    public int MaterialId { get; private set; }
    public int Quantity { get; private set; }

    public Workspace Workspace { get; private set; } = null!;
    public Material Material { get; private set; } = null!;

    private WorkspaceMaterial() { }

    public static WorkspaceMaterial Create(int workspaceId, int materialId, int quantity) =>
        new() { WorkspaceId = workspaceId, MaterialId = materialId, Quantity = quantity };
}
