using FluentAssertions;
using WorkTogetherly.Domain.Entities;

namespace TestWorkTogetherly.Domain;

public class MessageTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithSenderAndRecipient_SetsFields()
    {
        var senderId = Guid.NewGuid();
        var recipientId = Guid.NewGuid();

        var message = Message.Create(senderId, recipientId, "Bonjour");

        message.SenderId.Should().Be(senderId);
        message.RecipientId.Should().Be(recipientId);
        message.Content.Should().Be("Bonjour");
        message.IsRead.Should().BeFalse();
    }

    // ── MarkAsRead ────────────────────────────────────────────────────────────

    [Fact]
    public void MarkAsRead_SetsIsReadToTrue()
    {
        var message = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");

        message.MarkAsRead();

        message.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MarkAsRead_WhenCalledTwice_RemainsTrue()
    {
        var message = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Test");
        message.MarkAsRead();

        message.MarkAsRead();

        message.IsRead.Should().BeTrue();
    }
}
