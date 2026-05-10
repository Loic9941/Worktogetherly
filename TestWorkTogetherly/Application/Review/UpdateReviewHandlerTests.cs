using FluentAssertions;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Reviews.UpdateReview;
using WorkTogetherly.Domain.Interfaces;
using AppReviewErrors = WorkTogetherly.Application.Errors.ReviewErrors;
using DomainReviewErrors = WorkTogetherly.Domain.Errors.ReviewErrors;

namespace TestWorkTogetherly.Application.Review;

public class UpdateReviewHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IReviewRepository _reviewRepo = Substitute.For<IReviewRepository>();
    private readonly UpdateReviewHandler _handler;

    public UpdateReviewHandlerTests()
    {
        _handler = new UpdateReviewHandler(_reviewRepo);
    }

    [Fact]
    public async Task Handle_WhenReviewNotFound_ReturnsNotFound()
    {
        _reviewRepo.GetByIdAsync(1).Returns((WorkTogetherly.Domain.Entities.Review?)null);

        var result = await _handler.Handle(new UpdateReviewCommand(1, UserId, 5, "Super"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainReviewErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ReturnsNotOwner()
    {
        var review = EntityFactory.MakeReview(reviewerId: Guid.NewGuid());
        _reviewRepo.GetByIdAsync(1).Returns(review);

        var result = await _handler.Handle(new UpdateReviewCommand(1, UserId, 5, "Super"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppReviewErrors.NotOwner.Code);
    }

    [Fact]
    public async Task Handle_WhenRatingInvalid_ReturnsInvalidRating()
    {
        var review = EntityFactory.MakeReview(reviewerId: UserId);
        _reviewRepo.GetByIdAsync(1).Returns(review);

        var result = await _handler.Handle(new UpdateReviewCommand(1, UserId, 0, "Mauvais"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainReviewErrors.InvalidRating.Code);
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesReviewAndSaves()
    {
        var review = EntityFactory.MakeReview(reviewerId: UserId, rating: 2, comment: "Moyen");
        _reviewRepo.GetByIdAsync(1).Returns(review);

        var result = await _handler.Handle(new UpdateReviewCommand(1, UserId, 5, "Excellent"), default);

        result.IsError.Should().BeFalse();
        result.Value.Rating.Should().Be(5);
        result.Value.Comment.Should().Be("Excellent");
        await _reviewRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
