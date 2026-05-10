using FluentAssertions;
using WorkTogetherly.Domain.Entities;

namespace TestWorkTogetherly.Domain;

public class WorkspaceTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WhenValid_SetsFields()
    {
        var workspace = Workspace.Create(OwnerId, "Mon bureau", "Desc", "1 rue Test", 48.8, 2.3, 10, true);

        workspace.UserId.Should().Be(OwnerId);
        workspace.Name.Should().Be("Mon bureau");
        workspace.Capacity.Should().Be(10);
        workspace.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenCapacityZero_Throws()
    {
        var act = () => Workspace.Create(OwnerId, "Nom", "Desc", "Adresse", 48.8, 2.3, 0, true);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WhenCapacityNegative_Throws()
    {
        var act = () => Workspace.Create(OwnerId, "Nom", "Desc", "Adresse", 48.8, 2.3, -5, true);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WhenValid_MutatesFields()
    {
        var workspace = Workspace.Create(OwnerId, "Ancien nom", "Desc", "Adresse", 48.8, 2.3, 5, true);

        workspace.Update("Nouveau nom", "Nouvelle desc", "Nouvelle adresse", 49.0, 3.0, 20, false);

        workspace.Name.Should().Be("Nouveau nom");
        workspace.Capacity.Should().Be(20);
        workspace.IsActive.Should().BeFalse();
    }
}
