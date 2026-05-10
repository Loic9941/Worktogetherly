using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Bookings.Common;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Bookings.GetUserBookings
{
    public class GetUserBookingsHandler(IBookingRepository bookingRepository)
                : IRequestHandler<GetUserBookingsQuery, ErrorOr<IReadOnlyList<BookingResult>>>
    {
        private readonly IBookingRepository _bookingRepository = bookingRepository;

        public async Task<ErrorOr<IReadOnlyList<BookingResult>>> Handle(
            GetUserBookingsQuery request,
            CancellationToken cancellationToken)
        {
            // Fetch bookings with details for the specified user and date range
            var bookings = await _bookingRepository.GetByUserIdWithDetailsAsync(
                request.UserId, request.From, request.To, cancellationToken);

            var results = bookings.Select(b => b.ToResult()).ToList();

            return results;
        }
    }
}
