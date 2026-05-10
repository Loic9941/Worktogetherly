using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Bookings.Common;

namespace WorkTogetherly.Application.Bookings.CancelBooking
{
    public record CancelBookingCommand(int BookingId, Guid UserId) : IRequest<ErrorOr<BookingResult>>;
}
