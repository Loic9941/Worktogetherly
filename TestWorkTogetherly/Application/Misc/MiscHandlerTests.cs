using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Bookings.GetUserBookings;
using WorkTogetherly.Application.Messages.GetUserMessages;
using WorkTogetherly.Application.Messages.MarkMessageAsRead;
using WorkTogetherly.Application.Reviews.GetReviewByBooking;
using WorkTogetherly.Application.Reviews.GetWorkspaceReviews;
using WorkTogetherly.Application.Slots.GetSlotsByWorkspace;
using WorkTogetherly.Application.Slots.UpdateSlot;
using WorkTogetherly.Application.Interfaces;
using DomainSlot = WorkTogetherly.Domain.Entities.Slot;
using DomainWorkspace = WorkTogetherly.Domain.Entities.Workspace;
using DomainReview = WorkTogetherly.Domain.Entities.Review;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Interfaces;
using AppSlotErrors = WorkTogetherly.Application.Errors.SlotErrors;
using AppWorkspaceErrors = WorkTogetherly.Application.Errors.WorkspaceErrors;
using DomainSlotErrors = WorkTogetherly.Domain.Errors.SlotErrors;
using DomainWorkspaceErrors = WorkTogetherly.Domain.Errors.WorkspaceErrors;
using DomainMessageErrors = WorkTogetherly.Domain.Errors.MessageErrors;

namespace TestWorkTogetherly.Application.Misc;

public class MiscHandlerTests
{
    private static readonly Guid OwnerId = Guid.NewGuid();
    private static readonly Guid OtherId = Guid.NewGuid();
    private static readonly DateTime FutureStart = new(2030, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime FutureEnd = new(2030, 6, 1, 18, 0, 0, DateTimeKind.Utc);

    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly ISlotRepository _slotRepo = Substitute.For<ISlotRepository>();
    private readonly IWorkspaceRepository _workspaceRepo = Substitute.For<IWorkspaceRepository>();
    private readonly IReviewRepository _reviewRepo = Substitute.For<IReviewRepository>();
    private readonly IMessageRepository _messageRepo = Substitute.For<IMessageRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public MiscHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    // ── GetUserBookingsHandler ────────────────────────────────────────────────

    [Fact]
    public async Task GetUserBookings_WhenCalled_ReturnsMappedResults()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: workspace);
        var booking = EntityFactory.MakeBooking(userId: OwnerId, slot: slot);
        _bookingRepo.GetByUserIdWithDetailsAsync(OwnerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), default)
            .Returns([booking]);
        var handler = new GetUserBookingsHandler(_bookingRepo);

        var result = await handler.Handle(new GetUserBookingsQuery(OwnerId, DateTime.Today, DateTime.Today.AddDays(6)), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetUserBookings_WhenNoBookings_ReturnsEmptyList()
    {
        _bookingRepo.GetByUserIdWithDetailsAsync(OwnerId, Arg.Any<DateTime>(), Arg.Any<DateTime>(), default)
            .Returns([]);
        var handler = new GetUserBookingsHandler(_bookingRepo);

        var result = await handler.Handle(new GetUserBookingsQuery(OwnerId, DateTime.Today, DateTime.Today.AddDays(6)), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    // ── UpdateSlotHandler ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSlot_WhenSlotNotFound_ReturnsNotFound()
    {
        _slotRepo.GetByIdAsync(1, default).Returns((DomainSlot?)null);
        var handler = new UpdateSlotHandler(_workspaceRepo, _slotRepo, _clock);

        var result = await handler.Handle(new UpdateSlotCommand(1, 1, OwnerId, FutureStart, FutureEnd, 5), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainSlotErrors.NotFound.Code);
    }

    [Fact]
    public async Task UpdateSlot_WhenWorkspaceNotFound_ReturnsNotFound()
    {
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd);
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        _workspaceRepo.GetByIdAsync(1, default).Returns((DomainWorkspace?)null);
        var handler = new UpdateSlotHandler(_workspaceRepo, _slotRepo, _clock);

        var result = await handler.Handle(new UpdateSlotCommand(1, 1, OwnerId, FutureStart, FutureEnd, 5), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task UpdateSlot_WhenNotOwner_ReturnsUnauthorized()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: workspace);
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        _slotRepo.GetByWorkspaceIdAsync(1, default).Returns([]);
        var handler = new UpdateSlotHandler(_workspaceRepo, _slotRepo, _clock);

        var result = await handler.Handle(new UpdateSlotCommand(1, 1, OtherId, FutureStart, FutureEnd, 5), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppWorkspaceErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task UpdateSlot_WhenSlotAlreadyStarted_ReturnsAlreadyStarted()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var pastStart = _clock.UtcNow.AddHours(-2);
        var pastEnd = _clock.UtcNow.AddHours(-1);
        var slot = EntityFactory.MakeSlot(start: pastStart, end: pastEnd, workspace: workspace);
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        _slotRepo.GetByWorkspaceIdAsync(1, default).Returns([]);
        var handler = new UpdateSlotHandler(_workspaceRepo, _slotRepo, _clock);

        var result = await handler.Handle(new UpdateSlotCommand(1, 1, OwnerId, FutureStart, FutureEnd, 5), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppSlotErrors.AlreadyStarted.Code);
    }

    [Fact]
    public async Task UpdateSlot_WhenValid_UpdatesAndSaves()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var slot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd, workspace: workspace);
        _slotRepo.GetByIdAsync(1, default).Returns(slot);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        _slotRepo.GetByWorkspaceIdAsync(1, default).Returns([]);
        _slotRepo.GetByIdAsync(slot.Id, default).Returns(slot);
        var handler = new UpdateSlotHandler(_workspaceRepo, _slotRepo, _clock);

        var result = await handler.Handle(new UpdateSlotCommand(1, 1, OwnerId, FutureStart.AddHours(1), FutureEnd, 5), default);

        result.IsError.Should().BeFalse();
        await _slotRepo.Received(1).SaveChangesAsync(default);
    }

    // ── GetSlotsByWorkspaceHandler ────────────────────────────────────────────

    [Fact]
    public async Task GetSlotsByWorkspace_WhenWorkspaceNotFound_ReturnsNotFound()
    {
        _workspaceRepo.GetByIdAsync(1, default).Returns((DomainWorkspace?)null);
        var handler = new GetSlotsByWorkspaceHandler(_workspaceRepo, _slotRepo, _clock);

        var result = await handler.Handle(new GetSlotsByWorkspaceQuery(1), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainWorkspaceErrors.NotFound.Code);
    }

    [Fact]
    public async Task GetSlotsByWorkspace_WhenFound_ReturnsFutureNonCancelledSlots()
    {
        var workspace = EntityFactory.MakeWorkspace(ownerId: OwnerId);
        var futureSlot = EntityFactory.MakeSlot(start: FutureStart, end: FutureEnd);
        var pastSlot = EntityFactory.MakeSlot(id: 2, start: _clock.UtcNow.AddDays(-1), end: _clock.UtcNow.AddDays(-1).AddHours(8));
        var cancelledSlot = EntityFactory.MakeSlot(id: 3, start: FutureStart, end: FutureEnd, cancelled: true);
        _workspaceRepo.GetByIdAsync(1, default).Returns(workspace);
        _slotRepo.GetByWorkspaceIdAsync(1, default).Returns([futureSlot, pastSlot, cancelledSlot]);
        var handler = new GetSlotsByWorkspaceHandler(_workspaceRepo, _slotRepo, _clock);

        var result = await handler.Handle(new GetSlotsByWorkspaceQuery(1), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
    }

    // ── GetReviewByBookingHandler ─────────────────────────────────────────────

    [Fact]
    public async Task GetReviewByBooking_WhenNotFound_ReturnsNull()
    {
        _reviewRepo.GetByBookingIdAsync(1, default).Returns((DomainReview?)null);
        var handler = new GetReviewByBookingHandler(_reviewRepo);

        var result = await handler.Handle(new GetReviewByBookingQuery(1), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetReviewByBooking_WhenFound_ReturnsResult()
    {
        var review = EntityFactory.MakeReview();
        _reviewRepo.GetByBookingIdAsync(1, default).Returns(review);
        var handler = new GetReviewByBookingHandler(_reviewRepo);

        var result = await handler.Handle(new GetReviewByBookingQuery(1), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value!.Rating.Should().Be(review.Rating);
    }

    // ── GetWorkspaceReviewsHandler ────────────────────────────────────────────

    [Fact]
    public async Task GetWorkspaceReviews_WhenCalled_ReturnsMappedResults()
    {
        var review = EntityFactory.MakeReview(rating: 5);
        _reviewRepo.GetByWorkspaceIdAsync(1, default).Returns([review]);
        var handler = new GetWorkspaceReviewsHandler(_reviewRepo);

        var result = await handler.Handle(new GetWorkspaceReviewsQuery(1), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetWorkspaceReviews_WhenNoReviews_ReturnsEmptyList()
    {
        _reviewRepo.GetByWorkspaceIdAsync(1, default).Returns([]);
        var handler = new GetWorkspaceReviewsHandler(_reviewRepo);

        var result = await handler.Handle(new GetWorkspaceReviewsQuery(1), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    // ── GetUserMessagesHandler ────────────────────────────────────────────────

    [Fact]
    public async Task GetUserMessages_WhenCalled_ReturnsMappedResults()
    {
        var message = Message.Create(OwnerId, OtherId, "Bonjour");
        _messageRepo.GetByRecipientIdAsync(OtherId, default).Returns([message]);
        var handler = new GetUserMessagesHandler(_messageRepo);

        var result = await handler.Handle(new GetUserMessagesQuery(OtherId), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(1);
        result.Value[0].Content.Should().Be("Bonjour");
    }

    [Fact]
    public async Task GetUserMessages_WhenNoMessages_ReturnsEmptyList()
    {
        _messageRepo.GetByRecipientIdAsync(OtherId, default).Returns([]);
        var handler = new GetUserMessagesHandler(_messageRepo);

        var result = await handler.Handle(new GetUserMessagesQuery(OtherId), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    // ── MarkMessageAsReadHandler ──────────────────────────────────────────────

    [Fact]
    public async Task MarkMessageAsRead_WhenNotFound_ReturnsNotFound()
    {
        _messageRepo.GetByIdAsync(1, default).Returns((Message?)null);
        var handler = new MarkMessageAsReadHandler(_messageRepo);

        var result = await handler.Handle(new MarkMessageAsReadCommand(1, OtherId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainMessageErrors.NotFound.Code);
    }

    [Fact]
    public async Task MarkMessageAsRead_WhenNotRecipient_ReturnsNotFound()
    {
        var message = Message.Create(OwnerId, OtherId, "Test");
        _messageRepo.GetByIdAsync(1, default).Returns(message);
        var handler = new MarkMessageAsReadHandler(_messageRepo);

        var result = await handler.Handle(new MarkMessageAsReadCommand(1, OwnerId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainMessageErrors.NotFound.Code);
    }

    [Fact]
    public async Task MarkMessageAsRead_WhenValid_MarksAndSaves()
    {
        var message = Message.Create(OwnerId, OtherId, "Test");
        _messageRepo.GetByIdAsync(1, default).Returns(message);
        var handler = new MarkMessageAsReadHandler(_messageRepo);

        var result = await handler.Handle(new MarkMessageAsReadCommand(1, OtherId), default);

        result.IsError.Should().BeFalse();
        message.IsRead.Should().BeTrue();
        await _messageRepo.Received(1).SaveChangesAsync(default);
    }
}


