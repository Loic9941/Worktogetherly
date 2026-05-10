using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Bookings.Common;

namespace WorkTogetherly.Application.Bookings.GetUserBookings
{
    public record GetUserBookingsQuery(Guid UserId, DateTime From, DateTime To)
        : IRequest<ErrorOr<IReadOnlyList<BookingResult>>>;
}
