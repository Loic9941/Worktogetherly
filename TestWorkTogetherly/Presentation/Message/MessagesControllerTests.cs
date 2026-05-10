using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Messages.Common;
using WorkTogetherly.Application.Messages.GetUserMessages;
using WorkTogetherly.Application.Messages.MarkMessageAsRead;
using WorkTogetherly.Presentation.Controllers.Messages;

namespace TestWorkTogetherly.Presentation.Message;

public class MessagesControllerTests
{
    private static readonly MessageResult SampleMessage = new(
        Id: 1,
        Content: "Nouvelle réservation sur votre espace.",
        IsRead: false,
        CreatedAt: new DateTime(2025, 5, 1, 10, 0, 0));

    private static MessagesController CreateController(IMediator mediator, Guid? userId = null)
    {
        var controller = new MessagesController(mediator);
        var claims = userId.HasValue
            ? new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }
            : Array.Empty<Claim>();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims))
            }
        };
        return controller;
    }

    // ── GetMine ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMine_WhenHandlerSucceeds_ReturnsOkWithMessages()
    {
        var userId = Guid.NewGuid();
        var messages = new List<MessageResult> { SampleMessage };
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserMessagesQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<List<MessageResult>>)messages);

        var controller = CreateController(mediator, userId);

        var result = await controller.GetMine();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(messages);
    }

    [Fact]
    public async Task GetMine_WhenNoMessages_ReturnsOkWithEmptyList()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserMessagesQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<List<MessageResult>>)new List<MessageResult>());

        var controller = CreateController(mediator, userId);

        var result = await controller.GetMine();

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.As<List<MessageResult>>().Should().BeEmpty();
    }

    [Fact]
    public async Task GetMine_MapsUserIdToQuery()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetUserMessagesQuery>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<List<MessageResult>>)new List<MessageResult>());

        var controller = CreateController(mediator, userId);
        await controller.GetMine();

        await mediator.Received(1).Send(
            Arg.Is<GetUserMessagesQuery>(q => q.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    // ── MarkAsRead ────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkAsRead_WhenHandlerSucceeds_Returns204()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<MarkMessageAsReadCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<Success>)Result.Success);

        var controller = CreateController(mediator, userId);

        var result = await controller.MarkAsRead(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task MarkAsRead_WhenMessageNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<MarkMessageAsReadCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("Message.NotFound", "Message introuvable"));

        var controller = CreateController(mediator, userId);

        var result = await controller.MarkAsRead(99);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotRecipient_Returns403()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<MarkMessageAsReadCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Forbidden("Message.NotRecipient", "Pas le destinataire"));

        var controller = CreateController(mediator, userId);

        var result = await controller.MarkAsRead(1);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task MarkAsRead_MapsIdAndUserIdToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<MarkMessageAsReadCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<Success>)Result.Success);

        var controller = CreateController(mediator, userId);
        await controller.MarkAsRead(3);

        await mediator.Received(1).Send(
            Arg.Is<MarkMessageAsReadCommand>(c => c.MessageId == 3 && c.UserId == userId),
            Arg.Any<CancellationToken>());
    }
}
