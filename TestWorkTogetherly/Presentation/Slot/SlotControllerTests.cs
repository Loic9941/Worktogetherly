using ErrorOr;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using WorkTogetherly.Application.Slots.CancelSlot;
using WorkTogetherly.Application.Slots.Common;
using WorkTogetherly.Application.Slots.CreateSlot;
using WorkTogetherly.Application.Slots.GetSlotsByWorkspace;
using WorkTogetherly.Application.Slots.UpdateSlot;
using WorkTogetherly.Presentation.Controllers.Slot;
using WorkTogetherly.Presentation.Models.Slot;

namespace TestWorkTogetherly.Presentation.Slot;

public class SlotControllerTests
{
    private static readonly SlotResult SampleSlotResult = new(
        Id: 1,
        WorkspaceId: 10,
        StartDateTime: new DateTime(2025, 6, 1, 9, 0, 0),
        EndDateTime: new DateTime(2025, 6, 1, 17, 0, 0),
        Capacity: 5,
        AvailablePlaces: 3,
        Attendees: []);

    private static readonly CreateSlotRequest SampleCreateRequest = new(
        StartDateTime: new DateTime(2025, 6, 1, 9, 0, 0),
        EndDateTime: new DateTime(2025, 6, 1, 17, 0, 0),
        Capacity: 5);

    private static readonly UpdateSlotRequest SampleUpdateRequest = new(
        StartDateTime: new DateTime(2025, 6, 1, 10, 0, 0),
        EndDateTime: new DateTime(2025, 6, 1, 18, 0, 0),
        Capacity: 8);

    private static SlotController CreateController(IMediator mediator, Guid? userId = null)
    {
        var controller = new SlotController(mediator);
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

    // ── GetSlots ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSlots_WhenHandlerSucceeds_ReturnsOkWithList()
    {
        var slots = new List<SlotResult> { SampleSlotResult };
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSlotsByWorkspaceQuery>(), Arg.Any<CancellationToken>())
                .Returns(ErrorOrFactory.From<IReadOnlyList<SlotResult>>(slots));

        var controller = CreateController(mediator, Guid.NewGuid());

        var result = await controller.GetSlots(10);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(slots);
    }

    [Fact]
    public async Task GetSlots_WhenWorkspaceNotFound_Returns404()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSlotsByWorkspaceQuery>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("Workspace.NotFound", "Espace introuvable"));

        var controller = CreateController(mediator, Guid.NewGuid());

        var result = await controller.GetSlots(99);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetSlots_MapsWorkspaceIdToQuery()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetSlotsByWorkspaceQuery>(), Arg.Any<CancellationToken>())
                .Returns(ErrorOrFactory.From<IReadOnlyList<SlotResult>>(new List<SlotResult>()));

        var controller = CreateController(mediator, Guid.NewGuid());
        await controller.GetSlots(7);

        await mediator.Received(1).Send(
            Arg.Is<GetSlotsByWorkspaceQuery>(q => q.WorkspaceId == 7),
            Arg.Any<CancellationToken>());
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_WhenHandlerSucceeds_Returns201()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<SlotResult>)SampleSlotResult);

        var controller = CreateController(mediator, userId);

        var result = await controller.Create(10, SampleCreateRequest);

        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().Be(SampleSlotResult);
    }

    [Fact]
    public async Task Create_WhenUnauthorizedOwner_Returns403()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Forbidden("Workspace.Unauthorized", "Accès refusé"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Create(10, SampleCreateRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task Create_WhenValidationError_Returns400WithModelState()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Validation("EndDateTime", "La fin doit être après le début"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Create(10, SampleCreateRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task Create_MapsAllFieldsToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CreateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<SlotResult>)SampleSlotResult);

        var controller = CreateController(mediator, userId);
        await controller.Create(10, SampleCreateRequest);

        await mediator.Received(1).Send(
            Arg.Is<CreateSlotCommand>(c =>
                c.WorkspaceId == 10 &&
                c.UserId == userId &&
                c.StartDateTime == SampleCreateRequest.StartDateTime &&
                c.Capacity == SampleCreateRequest.Capacity),
            Arg.Any<CancellationToken>());
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_WhenHandlerSucceeds_ReturnsOkWithResult()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<SlotResult>)SampleSlotResult);

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(10, 1, SampleUpdateRequest);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(SampleSlotResult);
    }

    [Fact]
    public async Task Update_WhenSlotNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("Slot.NotFound", "Créneau introuvable"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(10, 99, SampleUpdateRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_WhenSlotAlreadyStarted_Returns409()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Conflict("Slot.AlreadyStarted", "Créneau déjà commencé"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Update(10, 1, SampleUpdateRequest);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Update_MapsAllFieldsToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<UpdateSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<SlotResult>)SampleSlotResult);

        var controller = CreateController(mediator, userId);
        await controller.Update(10, 3, SampleUpdateRequest);

        await mediator.Received(1).Send(
            Arg.Is<UpdateSlotCommand>(c =>
                c.SlotId == 3 &&
                c.WorkspaceId == 10 &&
                c.UserId == userId &&
                c.Capacity == SampleUpdateRequest.Capacity),
            Arg.Any<CancellationToken>());
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Cancel_WhenHandlerSucceeds_Returns204()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<Deleted>)Result.Deleted);

        var controller = CreateController(mediator, userId);

        var result = await controller.Cancel(10, 1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Cancel_WhenSlotNotFound_Returns404()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.NotFound("Slot.NotFound", "Créneau introuvable"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Cancel(10, 99);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Cancel_WhenAlreadyCancelled_Returns409()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns(Error.Conflict("Slot.AlreadyCancelled", "Déjà annulé"));

        var controller = CreateController(mediator, userId);

        var result = await controller.Cancel(10, 1);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task Cancel_MapsIdsToCommand()
    {
        var userId = Guid.NewGuid();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<CancelSlotCommand>(), Arg.Any<CancellationToken>())
                .Returns((ErrorOr<Deleted>)Result.Deleted);

        var controller = CreateController(mediator, userId);
        await controller.Cancel(10, 4);

        await mediator.Received(1).Send(
            Arg.Is<CancelSlotCommand>(c => c.SlotId == 4 && c.WorkspaceId == 10 && c.UserId == userId),
            Arg.Any<CancellationToken>());
    }
}
