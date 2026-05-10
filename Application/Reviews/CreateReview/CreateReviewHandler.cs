using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Reviews.Common;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppReviewErrors = WorkTogetherly.Application.Errors.ReviewErrors;

namespace WorkTogetherly.Application.Reviews.CreateReview
{
    public class CreateReviewHandler(IBookingRepository bookingRepository, IReviewRepository reviewRepository, IClock clock) : IRequestHandler<CreateReviewCommand, ErrorOr<ReviewResult>>
    {
        private readonly IBookingRepository _bookingRepository = bookingRepository;
        private readonly IReviewRepository _reviewRepository = reviewRepository;
        private readonly IClock _clock = clock;

        public async Task<ErrorOr<ReviewResult>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
                return BookingErrors.NotFound;

            if (booking.UserId != request.UserId)
                return AppReviewErrors.BookingNotOwned;

            if (!booking.IsSlotEnded(_clock.UtcNow))
                return AppReviewErrors.BookingNotPast;

            var existing = await _reviewRepository.GetByReviewerIdAndWorkspaceIdAsync(
                request.UserId, booking.Slot.WorkspaceId, cancellationToken);
            if (existing is not null)
                return AppReviewErrors.AlreadyExists;

            var reviewOrError = Review.Create(request.BookingId, request.UserId, booking.Slot.WorkspaceId, request.Rating, request.Comment);
            if (reviewOrError.IsError)
                return reviewOrError.Errors;

            var review = reviewOrError.Value;
            await _reviewRepository.AddAsync(review, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            return review.ToResult();
        }
    }
}
