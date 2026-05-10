using FluentAssertions;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Repositories;

namespace TestWorkTogetherly.Infrastructure;

public class ReviewRepositoryTests : IClassFixture<ContainerSQL>
{
    private readonly ContainerSQL _container;

    public ReviewRepositoryTests(ContainerSQL container)
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

    private async Task<(User reviewer, Booking booking, Workspace workspace)> SeedReviewPrerequisitesAsync()
    {
        var owner = CreateTestUser("Review", "Owner");
        var reviewer = CreateTestUser("Review", "User");
        _container._context.Users.AddRange(owner, reviewer);
        await _container._context.SaveChangesAsync();

        var workspace = Workspace.Create(owner.Id, "WS Reviews", "Desc", "4 rue Test", 48.85, 2.35, 10, true);
        _container._context.Workspaces.Add(workspace);
        await _container._context.SaveChangesAsync();

        var start = DateTime.UtcNow.AddDays(-2);
        var slot = Slot.Create(workspace.Id, start, start.AddHours(8), 5).Value;
        _container._context.Slots.Add(slot);
        await _container._context.SaveChangesAsync();

        var arrival = TimeOnly.FromDateTime(start.AddHours(1));
        var booking = Booking.Create(slot.Id, reviewer.Id, arrival, slot.StartDateTime, slot.EndDateTime).Value;
        _container._context.Bookings.Add(booking);
        await _container._context.SaveChangesAsync();

        return (reviewer, booking, workspace);
    }

    // ── GetByBookingIdAsync ──

    [Fact]
    public async Task GetByBookingIdAsync_WhenExists_ReturnsReview()
    {
        var (reviewer, booking, workspace) = await SeedReviewPrerequisitesAsync();
        var review = Review.Create(booking.Id, reviewer.Id, workspace.Id, 4, "Super endroit").Value;
        _container._context.Reviews.Add(review);
        await _container._context.SaveChangesAsync();

        var repo = new ReviewRepository(_container._context);
        var result = await repo.GetByBookingIdAsync(booking.Id);

        result.Should().NotBeNull();
        result!.Rating.Should().Be(4);
    }

    [Fact]
    public async Task GetByBookingIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new ReviewRepository(_container._context);
        var result = await repo.GetByBookingIdAsync(int.MaxValue);

        result.Should().BeNull();
    }

    // ── GetByReviewerIdAndWorkspaceIdAsync ──

    [Fact]
    public async Task GetByReviewerIdAndWorkspaceIdAsync_WhenExists_ReturnsReview()
    {
        var (reviewer, booking, workspace) = await SeedReviewPrerequisitesAsync();
        var review = Review.Create(booking.Id, reviewer.Id, workspace.Id, 5, "Excellent").Value;
        _container._context.Reviews.Add(review);
        await _container._context.SaveChangesAsync();

        var repo = new ReviewRepository(_container._context);
        var result = await repo.GetByReviewerIdAndWorkspaceIdAsync(reviewer.Id, workspace.Id);

        result.Should().NotBeNull();
        result!.ReviewerId.Should().Be(reviewer.Id);
    }

    [Fact]
    public async Task GetByReviewerIdAndWorkspaceIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new ReviewRepository(_container._context);
        var result = await repo.GetByReviewerIdAndWorkspaceIdAsync(Guid.NewGuid(), int.MaxValue);

        result.Should().BeNull();
    }

    // ── GetByWorkspaceIdAsync ──

    [Fact]
    public async Task GetByWorkspaceIdAsync_ReturnsAllReviewsForWorkspace()
    {
        var (reviewer, booking, workspace) = await SeedReviewPrerequisitesAsync();
        var review = Review.Create(booking.Id, reviewer.Id, workspace.Id, 3, "Correct").Value;
        _container._context.Reviews.Add(review);
        await _container._context.SaveChangesAsync();

        var repo = new ReviewRepository(_container._context);
        var results = await repo.GetByWorkspaceIdAsync(workspace.Id);

        results.Should().NotBeEmpty();
        results.Should().OnlyContain(r => r.WorkspaceId == workspace.Id);
    }
}
