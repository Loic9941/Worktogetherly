using FluentAssertions;
using TestProjectBackend;
using WorkTogetherly.Infrastructure.Repositories;

namespace TestWorkTogetherly.Infrastructure;

public class MaterialRuleRepositoryTests : IClassFixture<ContainerSQL>
{
    private readonly ContainerSQL _container;

    public MaterialRuleRepositoryTests(ContainerSQL container)
    {
        _container = container;
    }

    // ── MaterialRepository.GetAllAsync ────────────────────────────────────────

    [Fact]
    public async Task Material_GetAllAsync_ReturnsAllMaterialsOrderedByName()
    {
        var repo = new MaterialRepository(_container._context);
        var results = await repo.GetAllAsync();

        results.Should().NotBeEmpty().And.HaveCountGreaterThanOrEqualTo(7);
    }

    // ── MaterialRepository.GetByIdAsync ───────────────────────────────────────

    [Fact]
    public async Task Material_GetByIdAsync_WhenSeeded_ReturnsItem()
    {
        var repo = new MaterialRepository(_container._context);
        var result = await repo.GetByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Material_GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new MaterialRepository(_container._context);
        var result = await repo.GetByIdAsync(int.MaxValue);

        result.Should().BeNull();
    }

    // ── RuleRepository.GetAllAsync ────────────────────────────────────────────

    [Fact]
    public async Task Rule_GetAllAsync_ReturnsAllRulesOrderedByName()
    {
        var repo = new RuleRepository(_container._context);
        var results = await repo.GetAllAsync();

        results.Should().NotBeEmpty().And.HaveCountGreaterThanOrEqualTo(5);
    }

    // ── RuleRepository.GetByIdAsync ───────────────────────────────────────────

    [Fact]
    public async Task Rule_GetByIdAsync_WhenSeeded_ReturnsItem()
    {
        var repo = new RuleRepository(_container._context);
        var result = await repo.GetByIdAsync(1);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Rule_GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new RuleRepository(_container._context);
        var result = await repo.GetByIdAsync(int.MaxValue);

        result.Should().BeNull();
    }
}
