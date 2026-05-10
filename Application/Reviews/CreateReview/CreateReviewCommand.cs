using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Reviews.Common;

namespace WorkTogetherly.Application.Reviews.CreateReview
{
    public record CreateReviewCommand(int BookingId, Guid UserId, int Rating, string Comment)
        : IRequest<ErrorOr<ReviewResult>>;
}
