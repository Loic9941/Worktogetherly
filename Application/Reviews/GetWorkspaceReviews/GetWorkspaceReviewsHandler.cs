using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Reviews.Common;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Reviews.GetWorkspaceReviews
{
    public class GetWorkspaceReviewsHandler(IReviewRepository reviewRepository) : IRequestHandler<GetWorkspaceReviewsQuery, ErrorOr<IReadOnlyList<ReviewResult>>>
    {
        private readonly IReviewRepository _reviewRepository = reviewRepository;

        public async Task<ErrorOr<IReadOnlyList<ReviewResult>>> Handle(GetWorkspaceReviewsQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _reviewRepository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);

            var results = reviews.Select(r => r.ToResult()).ToList();

            return results;
        }
    }
}
