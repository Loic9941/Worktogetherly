using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Slots.CreateSlot;
using WorkTogetherly.Domain.Interfaces;
using AppSlotErrors = WorkTogetherly.Application.Errors.SlotErrors;
using AppWorkspaceErrors = WorkTogetherly.Application.Errors.WorkspaceErrors;
using DomainWorkspaceErrors = WorkTogetherly.Domain.Errors.WorkspaceErrors;

namespace TestWorkTogetherly.Application.Slot;

public class CreateSlotHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Start = DateTime.UtcNow.AddDays(1).Date.AddHours(8);
    private static readonly DateTime End = Start.AddHours(8);

    private readonly IWorkspaceRepository _workspaceRepo = Substitute.For<IWorkspaceRepository>();
    private readonly ISlotRepository _slotRepo = Substitute.For<ISlotRepository>();
    private readonly CreateSlotHandler _handler;

    public CreateSlotHandlerTests()
    {
        _handler = new CreateSlotHandler(_workspaceRepo, _slotRepo);
    }

    private CreateSlotCommand MakeCommand(DateTime? start = null, DateTime? end = null, int capacity = 5) =>
        new(WorkspaceId: 1, UserId: UserId, StartDateTime: start ?? Start, EndDateTime: end ?? End, Capacity: capacity);

    [Fact]
    public async Task Handle_WhenWorkspaceNotFound_ReturnsWorkspaceNotFound()
    {
        _workspaceRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ReturnsUnauthorized()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: Guid.NewGuid());
        _workspaceRepo.GetByIdAsync(1).Returns(workspace);
        _slotRepo.GetByWorkspaceIdAsync(1).Returns(Array.Empty<WorkTogetherly.Domain.Entities.Slot>());

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppWorkspaceErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task Handle_WhenSlotsOverlap_ReturnsOverlapping()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: UserId);
        _workspaceRepo.GetByIdAsync(1).Returns(workspace);

        // Existing slot overlaps with the new one
        var existingSlot = EntityFactory.MakeSlot(start: Start.AddHours(-1), end: Start.AddHours(2));
        _slotRepo.GetByWorkspaceIdAsync(1).Returns(new[] { existingSlot });

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppSlotErrors.Overlapping.Code);
    }

    [Fact]
    public async Task Handle_WhenValid_AddsSlotAndSaves()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: UserId);
        _workspaceRepo.GetByIdAsync(1).Returns(workspace);
        _slotRepo.GetByWorkspaceIdAsync(1).Returns(Array.Empty<WorkTogetherly.Domain.Entities.Slot>());
        _slotRepo.GetByIdAsync(0).Returns(EntityFactory.MakeSlot(start: Start, end: End));

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeFalse();
        await _slotRepo.Received(1).AddAsync(
            Arg.Any<WorkTogetherly.Domain.Entities.Slot>(), Arg.Any<CancellationToken>());
        await _slotRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
