using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Slots.CancelSlot;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Interfaces;
using AppWorkspaceErrors = WorkTogetherly.Application.Errors.WorkspaceErrors;
using DomainSlotErrors = WorkTogetherly.Domain.Errors.SlotErrors;
using DomainWorkspaceErrors = WorkTogetherly.Domain.Errors.WorkspaceErrors;

namespace TestWorkTogetherly.Application.Slot;

public class CancelSlotHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IWorkspaceRepository _workspaceRepo = Substitute.For<IWorkspaceRepository>();
    private readonly ISlotRepository _slotRepo = Substitute.For<ISlotRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CancelSlotHandler _handler;

    public CancelSlotHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
        _handler = new CancelSlotHandler(_workspaceRepo, _slotRepo, _clock);
    }

    [Fact]
    public async Task Handle_WhenSlotNotFound_ReturnsSlotNotFound()
    {
        _slotRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Slot?)null);

        var result = await _handler.Handle(new CancelSlotCommand(1, 1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainSlotErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkspaceNotFound_ReturnsWorkspaceNotFound()
    {
        var slot = EntityFactory.MakeSlot(start: DateTime.UtcNow.AddDays(1));
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);

        var result = await _handler.Handle(new CancelSlotCommand(1, 1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ReturnsUnauthorized()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: Guid.NewGuid());
        var slot = EntityFactory.MakeSlot(start: DateTime.UtcNow.AddDays(1), workspace: workspace);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(1).Returns(workspace);

        var result = await _handler.Handle(new CancelSlotCommand(1, 1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppWorkspaceErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task Handle_WhenSlotAlreadyStarted_ReturnsAlreadyStarted()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: UserId);
        var slot = EntityFactory.MakeSlot(
            start: new DateTime(2025, 6, 1, 11, 0, 0, DateTimeKind.Utc),
            end: new DateTime(2025, 6, 1, 19, 0, 0, DateTimeKind.Utc),
            workspace: workspace);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(1).Returns(workspace);

        var result = await _handler.Handle(new CancelSlotCommand(1, 1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainSlotErrors.AlreadyStarted.Code);
    }

    [Fact]
    public async Task Handle_WhenSlotAlreadyCancelled_ReturnsAlreadyCancelled()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: UserId);
        var slot = EntityFactory.MakeSlot(
            start: DateTime.UtcNow.AddDays(1),
            workspace: workspace,
            cancelled: true);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(1).Returns(workspace);

        var result = await _handler.Handle(new CancelSlotCommand(1, 1, UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainSlotErrors.AlreadyCancelled.Code);
    }

    [Fact]
    public async Task Handle_WhenValid_CancelsSlotAndSaves()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: UserId);
        var slot = EntityFactory.MakeSlot(
            start: DateTime.UtcNow.AddDays(1),
            workspace: workspace);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(1).Returns(workspace);

        var result = await _handler.Handle(new CancelSlotCommand(1, 1, UserId), default);

        result.IsError.Should().BeFalse();
        slot.IsCancelled.Should().BeTrue();
        await _slotRepo.Received(1).UpdateAsync(slot, Arg.Any<CancellationToken>());
        await _slotRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
