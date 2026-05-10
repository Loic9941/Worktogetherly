using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Reviews.Common;

namespace WorkTogetherly.Application.Reviews.GetReviewByBooking
{
    public record GetReviewByBookingQuery(int BookingId) : IRequest<ErrorOr<ReviewResult?>>;
}
