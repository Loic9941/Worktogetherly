using FluentAssertions;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Errors;

namespace TestWorkTogetherly.Domain;

public class ReviewTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void Create_WhenRatingValid_ReturnsReview(int rating)
    {
        var result = Review.Create(1, Guid.NewGuid(), 1, rating, "Bien");

        result.IsError.Should().BeFalse();
        result.Value.Rating.Should().Be(rating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public void Create_WhenRatingOutOfRange_ReturnsInvalidRating(int rating)
    {
        var result = Review.Create(1, Guid.NewGuid(), 1, rating, "Bien");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ReviewErrors.InvalidRating);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_WhenRatingValid_UpdatesFields()
    {
        var review = Review.Create(1, Guid.NewGuid(), 1, 3, "Moyen").Value;

        var result = review.Update(5, "Excellent");

        result.IsError.Should().BeFalse();
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Excellent");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Update_WhenRatingOutOfRange_ReturnsInvalidRating(int rating)
    {
        var review = Review.Create(1, Guid.NewGuid(), 1, 3, "Moyen").Value;

        var result = review.Update(rating, "X");

        result.IsError.Should().BeTrue();
        result.FirstError.Should().Be(ReviewErrors.InvalidRating);
    }
}
