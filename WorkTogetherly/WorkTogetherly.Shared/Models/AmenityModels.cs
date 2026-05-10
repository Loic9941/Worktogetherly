namespace WorkTogetherly.Shared.Models
{
    public record MaterialDto(int Id, string Name);

    public record RuleDto(int Id, string Name);

    public record WorkspaceMaterialDto(int MaterialId, string Name, int Quantity);

    public record WorkspaceRuleDto(int RuleId, string Name);

    public record SlotMaterialAvailabilityDto(int MaterialId, string Name, int TotalQuantity, int Available);
}
