using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Reviews.Common;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Reviews.GetReviewByBooking
{
    public class GetReviewByBookingHandler(IReviewRepository reviewRepository) : IRequestHandler<GetReviewByBookingQuery, ErrorOr<ReviewResult?>>
    {
        private readonly IReviewRepository _reviewRepository = reviewRepository;

        public async Task<ErrorOr<ReviewResult?>> Handle(GetReviewByBookingQuery request, CancellationToken cancellationToken)
        {
            // Fetch the review by booking ID
            var review = await _reviewRepository.GetByBookingIdAsync(request.BookingId, cancellationToken);
            if (review is null)
                return (ReviewResult?)null;

            return review.ToResult();
        }
    }
}
