using FluentAssertions;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Repositories;

namespace TestWorkTogetherly.Infrastructure;

public class WorkspaceRepositoryTests : IClassFixture<ContainerSQL>
{
    private readonly ContainerSQL _container;

    public WorkspaceRepositoryTests(ContainerSQL container)
    {
        _container = container;
    }

    private static User CreateTestUser(string firstName, string lastName)
    {
        var email = $"{firstName.ToLower()}_{Guid.NewGuid():N}@test.com";
        var user = User.Create(firstName, lastName, email);
        user.UserName = email;
        user.NormalizedUserName = email.ToUpperInvariant();
        user.NormalizedEmail = email.ToUpperInvariant();
        user.PasswordHash = "placeholder";
        user.SecurityStamp = Guid.NewGuid().ToString();
        return user;
    }

    private static Workspace CreateTestWorkspace(Guid userId, string name = "Mon espace", bool isActive = true) =>
        Workspace.Create(userId, name, "Description", "1 rue Test, Paris", 48.8566, 2.3522, 10, isActive);

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsWorkspaceWithNavigations()
    {
        var owner = CreateTestUser("Owner", "One");
        _container._context.Users.Add(owner);
        await _container._context.SaveChangesAsync();

        var workspace = CreateTestWorkspace(owner.Id, "Espace GetById");
        _container._context.Workspaces.Add(workspace);
        await _container._context.SaveChangesAsync();

        var repo = new WorkspaceRepository(_container._context);
        var result = await repo.GetByIdAsync(workspace.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Espace GetById");
        result.WorkspaceMaterials.Should().NotBeNull();
        result.WorkspaceRules.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new WorkspaceRepository(_container._context);
        var result = await repo.GetByIdAsync(int.MaxValue);

        result.Should().BeNull();
    }

    // ── GetByUserIdWithDetailsAsync ──

    [Fact]
    public async Task GetByUserIdWithDetailsAsync_WhenExists_ReturnsWorkspace()
    {
        var owner = CreateTestUser("Owner", "Details");
        _container._context.Users.Add(owner);
        await _container._context.SaveChangesAsync();

        var workspace = CreateTestWorkspace(owner.Id, "Espace Details");
        _container._context.Workspaces.Add(workspace);
        await _container._context.SaveChangesAsync();

        var repo = new WorkspaceRepository(_container._context);
        var result = await repo.GetByUserIdWithDetailsAsync(owner.Id);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(owner.Id);
    }

    // ── GetActiveAsync ──

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveWorkspaces()
    {
        var owner = CreateTestUser("Owner", "Active");
        _container._context.Users.Add(owner);
        await _container._context.SaveChangesAsync();

        _container._context.Workspaces.AddRange(
            CreateTestWorkspace(owner.Id, "Actif", isActive: true),
            CreateTestWorkspace(owner.Id, "Inactif", isActive: false));
        await _container._context.SaveChangesAsync();

        var repo = new WorkspaceRepository(_container._context);
        var results = await repo.GetActiveAsync();

        results.Should().OnlyContain(w => w.IsActive);
    }
}
