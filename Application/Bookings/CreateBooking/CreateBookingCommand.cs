using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Bookings.CreateBooking
{
    public record CreateBookingCommand(
        int SlotId,
        Guid UserId,
        TimeOnly ArrivalTime,
        IReadOnlyList<int> MaterialIds) : IRequest<ErrorOr<int>>;
}
