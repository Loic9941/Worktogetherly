using FluentAssertions;
using TestProjectBackend;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Repositories;

namespace TestWorkTogetherly.Infrastructure;

public class BookingRepositoryTests : IClassFixture<ContainerSQL>
{
    private readonly ContainerSQL _container;

    public BookingRepositoryTests(ContainerSQL container)
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

    private async Task<(User user, Slot slot)> SeedUserAndSlotAsync()
    {
        var owner = CreateTestUser("Booking", "Owner");
        var booker = CreateTestUser("Booking", "User");
        _container._context.Users.AddRange(owner, booker);
        await _container._context.SaveChangesAsync();

        var workspace = Workspace.Create(owner.Id, "WS Bookings", "Desc", "3 rue Test", 48.85, 2.35, 10, true);
        _container._context.Workspaces.Add(workspace);
        await _container._context.SaveChangesAsync();

        var start = DateTime.UtcNow.AddDays(1);
        var slot = Slot.Create(workspace.Id, start, start.AddHours(8), 5).Value;
        _container._context.Slots.Add(slot);
        await _container._context.SaveChangesAsync();

        return (booker, slot);
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsBookingWithSlotAndWorkspace()
    {
        var (user, slot) = await SeedUserAndSlotAsync();
        var arrival = TimeOnly.FromDateTime(slot.StartDateTime.AddHours(1));
        var booking = Booking.Create(slot.Id, user.Id, arrival, slot.StartDateTime, slot.EndDateTime).Value;
        _container._context.Bookings.Add(booking);
        await _container._context.SaveChangesAsync();

        var repo = new BookingRepository(_container._context);
        var result = await repo.GetByIdAsync(booking.Id);

        result.Should().NotBeNull();
        result!.Slot.Should().NotBeNull();
        result.Slot.Workspace.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var repo = new BookingRepository(_container._context);
        var result = await repo.GetByIdAsync(int.MaxValue);

        result.Should().BeNull();
    }

    // ── GetByUserIdWithDetailsAsync ──

    [Fact]
    public async Task GetByUserIdWithDetailsAsync_FiltersAndOrdersBySlotStart()
    {
        var (user, slot) = await SeedUserAndSlotAsync();
        var arrival = TimeOnly.FromDateTime(slot.StartDateTime.AddHours(1));
        var booking = Booking.Create(slot.Id, user.Id, arrival, slot.StartDateTime, slot.EndDateTime).Value;
        _container._context.Bookings.Add(booking);
        await _container._context.SaveChangesAsync();

        var repo = new BookingRepository(_container._context);
        var from = DateTime.UtcNow;
        var to = DateTime.UtcNow.AddDays(7);
        var results = await repo.GetByUserIdWithDetailsAsync(user.Id, from, to);

        results.Should().NotBeEmpty();
        results.Should().OnlyContain(b => b.UserId == user.Id);
        results.Should().BeInAscendingOrder(b => b.Slot.StartDateTime);
    }

    // ── CountActiveBySlotIdAsync ──

    [Fact]
    public async Task CountActiveBySlotIdAsync_ExcludesCancelledBookings()
    {
        var (user, slot) = await SeedUserAndSlotAsync();
        var arrival = TimeOnly.FromDateTime(slot.StartDateTime.AddHours(1));

        var active = Booking.Create(slot.Id, user.Id, arrival, slot.StartDateTime, slot.EndDateTime).Value;
        _container._context.Bookings.Add(active);
        await _container._context.SaveChangesAsync();

        var user2 = CreateTestUser("Cancel", "User");
        _container._context.Users.Add(user2);
        await _container._context.SaveChangesAsync();

        var cancelled = Booking.Create(slot.Id, user2.Id, arrival, slot.StartDateTime, slot.EndDateTime).Value;
        cancelled.Cancel();
        _container._context.Bookings.Add(cancelled);
        await _container._context.SaveChangesAsync();

        var repo = new BookingRepository(_container._context);
        var count = await repo.CountActiveBySlotIdAsync(slot.Id);

        count.Should().Be(1);
    }

    // ── CountActiveMaterialBookingsBySlotIdAsync ──

    [Fact]
    public async Task CountActiveMaterialBookingsBySlotIdAsync_CountsOnlyActiveBookingsWithMatchingMaterial()
    {
        var owner = CreateTestUser("Material", "Owner");
        _container._context.Users.Add(owner);
        await _container._context.SaveChangesAsync();

        var workspace = Workspace.Create(owner.Id, "WS Material", "Desc", "4 rue Test", 48.86, 2.36, 10, true);
        _container._context.Workspaces.Add(workspace);
        await _container._context.SaveChangesAsync();

        var material = Material.Create("Projecteur");
        _container._context.Materials.Add(material);
        await _container._context.SaveChangesAsync();

        workspace.ReplaceMaterials([(material.Id, 2)]);
        await _container._context.SaveChangesAsync();

        var start = DateTime.UtcNow.AddDays(2);
        var slot = Slot.Create(workspace.Id, start, start.AddHours(8), 5).Value;
        _container._context.Slots.Add(slot);
        await _container._context.SaveChangesAsync();

        // 2 active bookings with the material
        for (var i = 0; i < 2; i++)
        {
            var user = CreateTestUser("Material", $"User{i}");
            _container._context.Users.Add(user);
            await _container._context.SaveChangesAsync();

            var arrival = TimeOnly.FromDateTime(start.AddHours(1));
            var b = Booking.Create(slot.Id, user.Id, arrival, start, start.AddHours(8)).Value;
            b.AddMaterials([material.Id]);
            _container._context.Bookings.Add(b);
            await _container._context.SaveChangesAsync();
        }

        // 1 cancelled booking with the material — should not be counted
        var cancelUser = CreateTestUser("Material", "Cancel");
        _container._context.Users.Add(cancelUser);
        await _container._context.SaveChangesAsync();

        var cancelArrival = TimeOnly.FromDateTime(start.AddHours(1));
        var cancelled = Booking.Create(slot.Id, cancelUser.Id, cancelArrival, start, start.AddHours(8)).Value;
        cancelled.AddMaterials([material.Id]);
        cancelled.Cancel();
        _container._context.Bookings.Add(cancelled);
        await _container._context.SaveChangesAsync();

        var repo = new BookingRepository(_container._context);
        var count = await repo.CountActiveMaterialBookingsBySlotIdAsync(slot.Id, material.Id);

        count.Should().Be(2);
    }
}
