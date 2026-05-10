using FluentAssertions;
using TestProjectBackend;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Repositories;

namespace TestWorkTogetherly.Infrastructure;

public class SlotRepositoryTests : IClassFixture<ContainerSQL>
{
    private readonly ContainerSQL _container;

    public SlotRepositoryTests(ContainerSQL container)
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

    private async Task<Workspace> SeedWorkspaceAsync()
    {
        var owner = CreateTestUser("Slot", "Owner");
        _container._context.Users.Add(owner);
        await _container._context.SaveChangesAsync();

        var workspace = Workspace.Create(owner.Id, "Workspace Slots", "Desc", "2 rue Test", 48.85, 2.35, 5, true);
        _container._context.Workspaces.Add(workspace);
        await _container._context.SaveChangesAsync();
        return workspace;
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsSlotWithWorkspace()
    {
        var workspace = await SeedWorkspaceAsync();
        var start = DateTime.UtcNow.AddDays(1);
        var slot = Slot.Create(workspace.Id, start, start.AddHours(4), 3).Value;
        _container._context.Slots.Add(slot);
        await _container._context.SaveChangesAsync();

        var repo = new SlotRepository(_container._context);
        var result = await repo.GetByIdAsync(slot.Id);

        result.Should().NotBeNull();
        result!.Workspace.Should().NotBeNull();
        result.Capacity.Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new SlotRepository(_container._context);
        var result = await repo.GetByIdAsync(int.MaxValue);

        result.Should().BeNull();
    }

    // ── GetByWorkspaceIdAsync ──

    [Fact]
    public async Task GetByWorkspaceIdAsync_ReturnsSlotsSortedByStartDateTime()
    {
        var workspace = await SeedWorkspaceAsync();
        var baseDate = DateTime.UtcNow.AddDays(2);

        var laterSlot = Slot.Create(workspace.Id, baseDate.AddHours(8), baseDate.AddHours(12), 2).Value;
        var earlierSlot = Slot.Create(workspace.Id, baseDate, baseDate.AddHours(4), 2).Value;
        _container._context.Slots.AddRange(laterSlot, earlierSlot);
        await _container._context.SaveChangesAsync();

        var repo = new SlotRepository(_container._context);
        var results = await repo.GetByWorkspaceIdAsync(workspace.Id);

        results.Should().BeInAscendingOrder(s => s.StartDateTime);
    }
}
