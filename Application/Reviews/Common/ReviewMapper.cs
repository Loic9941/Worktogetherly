using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Reviews.Common;

public static class ReviewMapper
{
    public static ReviewResult ToResult(this Review review) => new(
        review.Id,
        review.BookingId,
        review.WorkspaceId,
        review.Rating,
        review.Comment,
        review.CreatedAt,
        review.Reviewer is not null
            ? $"{review.Reviewer.FirstName} {review.Reviewer.LastName}"
            : null);
}
