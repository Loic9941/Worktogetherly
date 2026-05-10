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

    // ── ReplacePhoto ──────────────────────────────────────────────────────────

    [Fact]
    public void ReplacePhoto_WhenNoExistingPhoto_SetsPathAndReturnsNull()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);

        var previous = workspace.ReplacePhoto("uploads/workspaces/1/photo.jpg");

        previous.Should().BeNull();
        workspace.PhotoPath.Should().Be("uploads/workspaces/1/photo.jpg");
    }

    [Fact]
    public void ReplacePhoto_WhenExistingPhoto_ReturnsOldPath()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);
        workspace.ReplacePhoto("old/photo.jpg");

        var previous = workspace.ReplacePhoto("new/photo.jpg");

        previous.Should().Be("old/photo.jpg");
        workspace.PhotoPath.Should().Be("new/photo.jpg");
    }

    // ── RemovePhoto ───────────────────────────────────────────────────────────

    [Fact]
    public void RemovePhoto_WhenPhotoExists_ClearsPathAndReturnsPrevious()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);
        workspace.ReplacePhoto("uploads/workspaces/1/photo.jpg");

        var previous = workspace.RemovePhoto();

        previous.Should().Be("uploads/workspaces/1/photo.jpg");
        workspace.PhotoPath.Should().BeNull();
    }

    [Fact]
    public void RemovePhoto_WhenNoPhoto_ReturnsNull()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);

        var previous = workspace.RemovePhoto();

        previous.Should().BeNull();
    }

    // ── ReplaceMaterials ──────────────────────────────────────────────────────

    [Fact]
    public void ReplaceMaterials_WhenCalled_ReplacesCollection()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);

        workspace.ReplaceMaterials([(1, 2), (3, 5)]);

        workspace.WorkspaceMaterials.Should().HaveCount(2);
        workspace.WorkspaceMaterials.Should().Contain(wm => wm.MaterialId == 1 && wm.Quantity == 2);
        workspace.WorkspaceMaterials.Should().Contain(wm => wm.MaterialId == 3 && wm.Quantity == 5);
    }

    [Fact]
    public void ReplaceMaterials_WhenCalledTwice_FullyReplacesPreviousCollection()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);
        workspace.ReplaceMaterials([(1, 2)]);

        workspace.ReplaceMaterials([(7, 3)]);

        workspace.WorkspaceMaterials.Should().HaveCount(1);
        workspace.WorkspaceMaterials.Should().Contain(wm => wm.MaterialId == 7 && wm.Quantity == 3);
    }

    [Fact]
    public void ReplaceMaterials_WhenEmpty_ClearsCollection()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);
        workspace.ReplaceMaterials([(1, 2)]);

        workspace.ReplaceMaterials([]);

        workspace.WorkspaceMaterials.Should().BeEmpty();
    }

    // ── ReplaceRules ──────────────────────────────────────────────────────────

    [Fact]
    public void ReplaceRules_WhenCalled_ReplacesCollection()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);

        workspace.ReplaceRules([1, 2, 3]);

        workspace.WorkspaceRules.Should().HaveCount(3);
        workspace.WorkspaceRules.Should().Contain(wr => wr.RuleId == 1);
        workspace.WorkspaceRules.Should().Contain(wr => wr.RuleId == 3);
    }

    [Fact]
    public void ReplaceRules_WhenCalledTwice_FullyReplacesPreviousCollection()
    {
        var workspace = Workspace.Create(OwnerId, "WS", "Desc", "Adresse", 48.8, 2.3, 5, true);
        workspace.ReplaceRules([1, 2]);

        workspace.ReplaceRules([5]);

        workspace.WorkspaceRules.Should().HaveCount(1);
        workspace.WorkspaceRules.Should().Contain(wr => wr.RuleId == 5);
    }
}
