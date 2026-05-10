using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Bookings.Common;

namespace WorkTogetherly.Application.Bookings.UpdateBookingArrivalTime
{
    public record UpdateBookingArrivalTimeCommand(int BookingId, Guid UserId, TimeOnly NewArrivalTime)
        : IRequest<ErrorOr<BookingResult>>;
}
