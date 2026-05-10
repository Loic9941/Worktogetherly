using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Bookings.CreateBooking;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Interfaces;
using AppBookingErrors = WorkTogetherly.Application.Errors.BookingErrors;
using AppSlotErrors = WorkTogetherly.Application.Errors.SlotErrors;
using DomainBookingErrors = WorkTogetherly.Domain.Errors.BookingErrors;
using DomainSlotErrors = WorkTogetherly.Domain.Errors.SlotErrors;
using DomainWorkspaceErrors = WorkTogetherly.Domain.Errors.WorkspaceErrors;

namespace TestWorkTogetherly.Application.Booking;

public class CreateBookingHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly DateTime FutureStart = DateTime.UtcNow.AddDays(1).Date.AddHours(8);
    private static readonly DateTime FutureEnd = FutureStart.AddHours(10);

    private readonly ISlotRepository _slotRepo = Substitute.For<ISlotRepository>();
    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly IWorkspaceRepository _workspaceRepo = Substitute.For<IWorkspaceRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CreateBookingHandler _handler;

    public CreateBookingHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
        _handler = new CreateBookingHandler(_slotRepo, _bookingRepo, _workspaceRepo, _clock);
    }

    private CreateBookingCommand MakeCommand(TimeOnly? arrivalTime = null) =>
        new(SlotId: 1, UserId: UserId, ArrivalTime: arrivalTime ?? new TimeOnly(9, 0), MaterialIds: []);

    [Fact]
    public async Task Handle_WhenSlotNotFound_ReturnsSlotNotFound()
    {
        _slotRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Slot?)null);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainSlotErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenSlotInThePast_ReturnsInThePast()
    {
        var pastSlot = EntityFactory.MakeSlot(start: DateTime.UtcNow.AddHours(-2), end: DateTime.UtcNow.AddHours(-1));
        _slotRepo.GetByIdAsync(1).Returns(pastSlot);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppSlotErrors.InThePast.Code);
    }

    [Fact]
    public async Task Handle_WhenWorkspaceNotFound_ReturnsWorkspaceNotFound()
    {
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(slot.WorkspaceId).Returns((WorkTogetherly.Domain.Entities.Workspace?)null);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserIsOwner_ReturnsOwnerCannotBook()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: UserId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: workspace);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(slot.WorkspaceId).Returns(workspace);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppBookingErrors.OwnerCannotBook.Code);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyBooked_ReturnsAlreadyBooked()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: workspace);
        var existingBooking = EntityFactory.MakeBooking(userId: UserId, slot: slot);
        slot.Bookings.Add(existingBooking);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(slot.WorkspaceId).Returns(workspace);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppBookingErrors.AlreadyBooked.Code);
    }

    [Fact]
    public async Task Handle_WhenSlotFull_ReturnsSlotFull()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, capacity: 1, workspace: workspace);
        var otherBooking = EntityFactory.MakeBooking(userId: Guid.NewGuid(), slot: slot);
        slot.Bookings.Add(otherBooking);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(slot.WorkspaceId).Returns(workspace);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppSlotErrors.SlotFull.Code);
    }

    [Fact]
    public async Task Handle_WhenArrivalTimeOutOfRange_ReturnsArrivalTimeOutOfRange()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: workspace);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(slot.WorkspaceId).Returns(workspace);

        var result = await _handler.Handle(MakeCommand(new TimeOnly(23, 0)), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainBookingErrors.ArrivalTimeOutOfRange.Code);
    }

    [Fact]
    public async Task Handle_WhenValid_AddsBookingAndReturnsId()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, capacity: 5, workspace: workspace);
        _slotRepo.GetByIdAsync(1).Returns(slot);
        _workspaceRepo.GetByIdAsync(slot.WorkspaceId).Returns(workspace);

        var result = await _handler.Handle(MakeCommand(new TimeOnly(9, 0)), default);

        result.IsError.Should().BeFalse();
        await _bookingRepo.Received(1).AddAsync(
            Arg.Any<WorkTogetherly.Domain.Entities.Booking>(), Arg.Any<CancellationToken>());
        await _bookingRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
