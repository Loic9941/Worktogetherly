using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Reviews.Common;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppReviewErrors = WorkTogetherly.Application.Errors.ReviewErrors;

namespace WorkTogetherly.Application.Reviews.UpdateReview
{
    public class UpdateReviewHandler(IReviewRepository reviewRepository) : IRequestHandler<UpdateReviewCommand, ErrorOr<ReviewResult>>
    {
        private readonly IReviewRepository _reviewRepository = reviewRepository;

        public async Task<ErrorOr<ReviewResult>> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
        {
            // Fetch the review by ID
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
            if (review is null)
                return ReviewErrors.NotFound;

            // Validate that the user is the owner of the review
            if (review.ReviewerId != request.UserId)
                return AppReviewErrors.NotOwner;

            // Update the review with new rating and comment
            var updateResult = review.Update(request.Rating, request.Comment);
            if (updateResult.IsError)
                return updateResult.Errors;

            // Persist changes
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            // Return the updated review result
            return review.ToResult();
        }
    }
}
