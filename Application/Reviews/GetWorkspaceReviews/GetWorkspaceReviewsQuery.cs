using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Reviews.Common;

namespace WorkTogetherly.Application.Reviews.GetWorkspaceReviews
{
    public record GetWorkspaceReviewsQuery(int WorkspaceId) : IRequest<ErrorOr<IReadOnlyList<ReviewResult>>>;
}
