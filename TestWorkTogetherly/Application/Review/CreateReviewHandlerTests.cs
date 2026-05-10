using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Reviews.CreateReview;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Interfaces;
using AppReviewErrors = WorkTogetherly.Application.Errors.ReviewErrors;
using DomainBookingErrors = WorkTogetherly.Domain.Errors.BookingErrors;
using DomainReviewErrors = WorkTogetherly.Domain.Errors.ReviewErrors;

namespace TestWorkTogetherly.Application.Review;

public class CreateReviewHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IBookingRepository _bookingRepo = Substitute.For<IBookingRepository>();
    private readonly IReviewRepository _reviewRepo = Substitute.For<IReviewRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CreateReviewHandler _handler;

    public CreateReviewHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc));
        _handler = new CreateReviewHandler(_bookingRepo, _reviewRepo, _clock);
    }

    private CreateReviewCommand MakeCommand(int rating = 4) =>
        new(BookingId: 1, UserId: UserId, Rating: rating, Comment: "Très bien");

    [Fact]
    public async Task Handle_WhenBookingNotFound_ReturnsBookingNotFound()
    {
        _bookingRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Booking?)null);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainBookingErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenBookingNotOwnedByUser_ReturnsBookingNotOwned()
    {
        var slot = EntityFactory.MakeSlot(end: DateTime.UtcNow.AddHours(-1));
        var booking = EntityFactory.MakeBooking(userId: Guid.NewGuid(), slot: slot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppReviewErrors.BookingNotOwned.Code);
    }

    [Fact]
    public async Task Handle_WhenSlotNotYetFinished_ReturnsBookingNotPast()
    {
        // Slot ends in the future → cannot review yet
        var futureSlot = EntityFactory.MakeSlot(
            start: DateTime.UtcNow.AddHours(1),
            end: DateTime.UtcNow.AddHours(9));
        var booking = EntityFactory.MakeBooking(userId: UserId, slot: futureSlot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppReviewErrors.BookingNotPast.Code);
    }

    [Fact]
    public async Task Handle_WhenReviewAlreadyExistsForWorkspace_ReturnsAlreadyExists()
    {
        var pastSlot = EntityFactory.MakeSlot(
            start: new DateTime(2025, 5, 29, 8, 0, 0, DateTimeKind.Utc),
            end: new DateTime(2025, 5, 31, 18, 0, 0, DateTimeKind.Utc));
        var booking = EntityFactory.MakeBooking(userId: UserId, slot: pastSlot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);

        var existingReview = EntityFactory.MakeReview(reviewerId: UserId);
        _reviewRepo.GetByReviewerIdAndWorkspaceIdAsync(UserId, Arg.Any<int>()).Returns(existingReview);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppReviewErrors.AlreadyExists.Code);
    }

    [Fact]
    public async Task Handle_WhenRatingInvalid_ReturnsInvalidRating()
    {
        var pastSlot = EntityFactory.MakeSlot(
            start: new DateTime(2025, 5, 29, 8, 0, 0, DateTimeKind.Utc),
            end: new DateTime(2025, 5, 31, 18, 0, 0, DateTimeKind.Utc));
        var booking = EntityFactory.MakeBooking(userId: UserId, slot: pastSlot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);
        _reviewRepo.GetByReviewerIdAndWorkspaceIdAsync(UserId, Arg.Any<int>()).Returns((WorkTogetherly.Domain.Entities.Review?)null);

        var result = await _handler.Handle(MakeCommand(rating: 6), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainReviewErrors.InvalidRating.Code);
    }

    [Fact]
    public async Task Handle_WhenValid_AddsReviewAndSaves()
    {
        var pastSlot = EntityFactory.MakeSlot(
            start: new DateTime(2025, 5, 29, 8, 0, 0, DateTimeKind.Utc),
            end: new DateTime(2025, 5, 31, 18, 0, 0, DateTimeKind.Utc));
        var booking = EntityFactory.MakeBooking(userId: UserId, slot: pastSlot);
        _bookingRepo.GetByIdAsync(1).Returns(booking);
        _reviewRepo.GetByReviewerIdAndWorkspaceIdAsync(UserId, Arg.Any<int>()).Returns((WorkTogetherly.Domain.Entities.Review?)null);

        var result = await _handler.Handle(MakeCommand(), default);

        result.IsError.Should().BeFalse();
        result.Value.Rating.Should().Be(4);
        await _reviewRepo.Received(1).AddAsync(
            Arg.Any<WorkTogetherly.Domain.Entities.Review>(), Arg.Any<CancellationToken>());
        await _reviewRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
