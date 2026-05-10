using FluentAssertions;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Repositories;

namespace TestWorkTogetherly.Infrastructure;

public class UserRepositoryTests : IClassFixture<ContainerSQL>
{
    private readonly ContainerSQL _container;

    public UserRepositoryTests(ContainerSQL container)
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

    // ── GetByEmailAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmailAsync_WhenExists_ReturnsUser()
    {
        var user = CreateTestUser("Alice", "GetEmail");
        _container._context.Users.Add(user);
        await _container._context.SaveChangesAsync();

        var repo = new UserRepository(_container._context, null!);
        var result = await repo.GetByEmailAsync(user.Email!);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task GetByEmailAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new UserRepository(_container._context, null!);
        var result = await repo.GetByEmailAsync("nonexistent@test.com");

        result.Should().BeNull();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsUser()
    {
        var user = CreateTestUser("Bob", "GetById");
        _container._context.Users.Add(user);
        await _container._context.SaveChangesAsync();

        var repo = new UserRepository(_container._context, null!);
        var result = await repo.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new UserRepository(_container._context, null!);
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── ExistsByEmailAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsByEmailAsync_WhenExists_ReturnsTrue()
    {
        var user = CreateTestUser("Carol", "Exists");
        _container._context.Users.Add(user);
        await _container._context.SaveChangesAsync();

        var repo = new UserRepository(_container._context, null!);
        var result = await repo.ExistsByEmailAsync(user.Email!);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenNotFound_ReturnsFalse()
    {
        var repo = new UserRepository(_container._context, null!);
        var result = await repo.ExistsByEmailAsync("nobody@test.com");

        result.Should().BeFalse();
    }
}
