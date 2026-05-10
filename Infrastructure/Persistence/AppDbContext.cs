using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Primitives;

namespace WorkTogetherly.Infrastructure.Persistence;

public class AppDbContext : IdentityUserContext<User, Guid>
{
    private readonly IPublisher _publisher;

    public AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher) : base(options)
    {
        _publisher = publisher;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Workspace> Workspaces { get; set; } = null!;
    public DbSet<Material> Materials { get; set; } = null!;
    public DbSet<Rule> Rules { get; set; } = null!;
    public DbSet<WorkspaceMaterial> WorkspaceMaterials { get; set; } = null!;
    public DbSet<WorkspaceRule> WorkspaceRules { get; set; } = null!;
    public DbSet<BookingMaterial> BookingMaterials { get; set; } = null!;
    public DbSet<Slot> Slots { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = CollectDomainEvents();
        var result = await base.SaveChangesAsync(cancellationToken);
        await PublishDomainEventsAsync(domainEvents, cancellationToken);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // This method collects all domain events from the tracked entities that are of type AggregateRoot.
    private List<Domain.Events.IDomainEvent> CollectDomainEvents()
    {
        return ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(entry => entry.Entity.PopDomainEvents())
            .ToList();
    }

    // This method publishes domain events after the changes have been saved to the database.
    private async Task PublishDomainEventsAsync(
        IEnumerable<Domain.Events.IDomainEvent> domainEvents,
        CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
            await _publisher.Publish(domainEvent, cancellationToken);
    }
}
