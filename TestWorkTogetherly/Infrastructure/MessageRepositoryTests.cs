using FluentAssertions;
using TestProjectBackend;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Infrastructure.Repositories;

namespace TestWorkTogetherly.Infrastructure;

public class MessageRepositoryTests : IClassFixture<ContainerSQL>
{
    private readonly ContainerSQL _container;

    public MessageRepositoryTests(ContainerSQL container)
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

    [Fact]
    public async Task GetByRecipientIdAsync_WhenMessagesExist_ReturnsThemOrderedByDateDesc()
    {
        var sender = CreateTestUser("Alice", "Smith");
        var recipient = CreateTestUser("Bob", "Jones");
        _container._context.Users.AddRange(sender, recipient);
        await _container._context.SaveChangesAsync();

        var older = Message.Create(sender.Id, recipient.Id, "Premier message");
        _container._context.Messages.Add(older);
        await _container._context.SaveChangesAsync();

        await Task.Delay(1);

        var newer = Message.Create(sender.Id, recipient.Id, "Deuxième message");
        _container._context.Messages.Add(newer);
        await _container._context.SaveChangesAsync();

        var repo = new MessageRepository(_container._context);
        var results = await repo.GetByRecipientIdAsync(recipient.Id);

        results.Should().HaveCount(2);
        results[0].Content.Should().Be("Deuxième message");
        results[1].Content.Should().Be("Premier message");
    }
}
