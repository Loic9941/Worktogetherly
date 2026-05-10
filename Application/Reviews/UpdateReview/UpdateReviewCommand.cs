using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Reviews.Common;

namespace WorkTogetherly.Application.Reviews.UpdateReview
{
    public record UpdateReviewCommand(int ReviewId, Guid UserId, int Rating, string Comment)
        : IRequest<ErrorOr<ReviewResult>>;
}
