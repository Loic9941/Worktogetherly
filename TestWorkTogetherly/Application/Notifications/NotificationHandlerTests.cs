using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Notifications.Bookings;
using WorkTogetherly.Application.Notifications.Slots;
using WorkTogetherly.Domain.Events.Bookings;
using WorkTogetherly.Domain.Events.Slots;
using WorkTogetherly.Domain.Interfaces;
using DomainUser = WorkTogetherly.Domain.Entities.User;
using DomainSlot = WorkTogetherly.Domain.Entities.Slot;
using DomainWorkspace = WorkTogetherly.Domain.Entities.Workspace;
using DomainBooking = WorkTogetherly.Domain.Entities.Booking;using WorkTogetherly.Domain.Entities;

namespace TestWorkTogetherly.Application.Notifications;

public class NotificationHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid BookerId = Guid.NewGuid();
    private static readonly DateTime FutureStart = new(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureEnd = new(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);

    private readonly ISlotRepository _slotRepo = Substitute.For<ISlotRepository>();
    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly IMessageRepository _messageRepo = Substitute.For<IMessageRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();

    private DomainWorkspace MakeWorkspace() => EntityFactory.MakeWorkspace(ownerId: OwnerId);

    private DomainSlot MakeSlotWithWorkspace()
    {
        var ws = MakeWorkspace();
        return EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: ws);
    }

    private DomainUser MakeOwner() =>
        DomainUser.Create("Owner", "Test", "owner@test.com");

    // ── BookingCreatedHandler ─────────────────────────────────────────────────

    [Fact]
    public async Task BookingCreated_WhenSlotNotFound_DoesNothing()
    {
        _slotRepo.GetByIdAsync(1, default).Returns((DomainSlot?)null);
        var handler = new BookingCreatedHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new BookingCreatedEvent(1, 1, BookerId), default);

        await _messageRepo.DidNotReceive().AddAsync(Arg.Any<Message>(), default);
    }

    [Fact]
    public async Task BookingCreated_WhenValid_SavesMessageAndNotifies()
    {
        var slot = MakeSlotWithWorkspace();
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        var owner = MakeOwner();
        _userRepo.GetByIdAsync(OwnerId, default).Returns(owner);
        var handler = new BookingCreatedHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new BookingCreatedEvent(1, 1, BookerId), default);

        await _messageRepo.Received(1).AddAsync(Arg.Any<Message>(), default);
        await _messageRepo.Received(1).SaveChangesAsync(default);
        await _notificationService.Received(1).SendToUserAsync(OwnerId, Arg.Any<WorkTogetherly.Application.Messages.Common.MessageResult>(), default);
    }

    [Fact]
    public async Task BookingCreated_WhenOwnerHasEmail_SendsEmail()
    {
        var slot = MakeSlotWithWorkspace();
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        var owner = MakeOwner();
        _userRepo.GetByIdAsync(OwnerId, default).Returns(owner);
        var handler = new BookingCreatedHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new BookingCreatedEvent(1, 1, BookerId), default);

        await _emailService.Received(1).SendAsync(owner.Email!, Arg.Any<string>(), Arg.Any<string>(), default);
    }

    // ── BookingCancelledHandler ───────────────────────────────────────────────

    [Fact]
    public async Task BookingCancelled_WhenSlotNotFound_DoesNothing()
    {
        _slotRepo.GetByIdAsync(1, default).Returns((DomainSlot?)null);
        var handler = new BookingCancelledHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new BookingCancelledEvent(1, 1, BookerId), default);

        await _messageRepo.DidNotReceive().AddAsync(Arg.Any<Message>(), default);
    }

    [Fact]
    public async Task BookingCancelled_WhenValid_SavesMessageAndNotifiesOwner()
    {
        var slot = MakeSlotWithWorkspace();
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        var owner = MakeOwner();
        _userRepo.GetByIdAsync(OwnerId, default).Returns(owner);
        var handler = new BookingCancelledHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new BookingCancelledEvent(1, 1, BookerId), default);

        await _messageRepo.Received(1).AddAsync(Arg.Any<Message>(), default);
        await _notificationService.Received(1).SendToUserAsync(OwnerId, Arg.Any<WorkTogetherly.Application.Messages.Common.MessageResult>(), default);
    }

    // ── BookingArrivalTimeUpdatedHandler ──────────────────────────────────────

    [Fact]
    public async Task BookingArrivalTimeUpdated_WhenBookingNotFound_DoesNothing()
    {
        _bookingRepo.GetByIdAsync(1, default).Returns((DomainBooking?)null);
        var handler = new BookingArrivalTimeUpdatedHandler(_bookingRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new BookingArrivalTimeUpdatedEvent(1, new TimeOnly(10, 0)), default);

        await _messageRepo.DidNotReceive().AddAsync(Arg.Any<Message>(), default);
    }

    [Fact]
    public async Task BookingArrivalTimeUpdated_WhenValid_SavesMessageAndNotifiesOwner()
    {
        var ws = MakeWorkspace();
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: ws);
        var booking = EntityFactory.MakeBooking(userId: BookerId, slot: slot);
        _bookingRepo.GetByIdAsync(1, default).Returns(booking);
        var owner = MakeOwner();
        _userRepo.GetByIdAsync(OwnerId, default).Returns(owner);
        var handler = new BookingArrivalTimeUpdatedHandler(_bookingRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new BookingArrivalTimeUpdatedEvent(1, new TimeOnly(10, 0)), default);

        await _messageRepo.Received(1).AddAsync(Arg.Any<Message>(), default);
        await _notificationService.Received(1).SendToUserAsync(OwnerId, Arg.Any<WorkTogetherly.Application.Messages.Common.MessageResult>(), default);
    }

    // ── SlotCancelledHandler ──────────────────────────────────────────────────

    [Fact]
    public async Task SlotCancelled_WhenSlotNotFound_DoesNothing()
    {
        _slotRepo.GetByIdAsync(1, default).Returns((DomainSlot?)null);
        var handler = new SlotCancelledHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new SlotCancelledEvent(1, 1), default);

        await _messageRepo.DidNotReceive().AddAsync(Arg.Any<Message>(), default);
    }

    [Fact]
    public async Task SlotCancelled_WhenNoActiveBookers_SendsNoMessages()
    {
        var slot = MakeSlotWithWorkspace();
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        var handler = new SlotCancelledHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new SlotCancelledEvent(1, 1), default);

        await _messageRepo.DidNotReceive().AddAsync(Arg.Any<Message>(), default);
    }

    [Fact]
    public async Task SlotCancelled_WhenActiveBookers_SendsMessageToEach()
    {
        var ws = MakeWorkspace();
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: ws);
        var booking = EntityFactory.MakeBooking(userId: BookerId, slot: slot);
        slot.Bookings.Add(booking);
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        var booker = DomainUser.Create("Booker", "Test", "booker@test.com");
        _userRepo.GetByIdAsync(BookerId, default).Returns(booker);
        var handler = new SlotCancelledHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new SlotCancelledEvent(1, 1), default);

        await _messageRepo.Received(1).AddAsync(Arg.Any<Message>(), default);
        await _notificationService.Received(1).SendToUserAsync(BookerId, Arg.Any<WorkTogetherly.Application.Messages.Common.MessageResult>(), default);
    }

    // ── SlotUpdatedHandler ────────────────────────────────────────────────────

    [Fact]
    public async Task SlotUpdated_WhenSlotNotFound_DoesNothing()
    {
        _slotRepo.GetByIdAsync(1, default).Returns((DomainSlot?)null);
        var handler = new SlotUpdatedHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new SlotUpdatedEvent(1, 1), default);

        await _messageRepo.DidNotReceive().AddAsync(Arg.Any<Message>(), default);
    }

    [Fact]
    public async Task SlotUpdated_WhenActiveBookers_SendsMessageToEach()
    {
        var ws = MakeWorkspace();
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: ws);
        var booking = EntityFactory.MakeBooking(userId: BookerId, slot: slot);
        slot.Bookings.Add(booking);
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        var booker = DomainUser.Create("Booker", "Test", "booker@test.com");
        _userRepo.GetByIdAsync(BookerId, default).Returns(booker);
        var handler = new SlotUpdatedHandler(_slotRepo, _messageRepo, _notificationService, _userRepo, _emailService);

        await handler.Handle(new SlotUpdatedEvent(1, 1), default);

        await _messageRepo.Received(1).AddAsync(Arg.Any<Message>(), default);
        await _notificationService.Received(1).SendToUserAsync(BookerId, Arg.Any<WorkTogetherly.Application.Messages.Common.MessageResult>(), default);
    }
}



