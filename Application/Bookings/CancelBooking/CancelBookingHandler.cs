using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Bookings.Common;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppBookingErrors = WorkTogetherly.Application.Errors.BookingErrors;

namespace WorkTogetherly.Application.Bookings.CancelBooking
{
    public class CancelBookingHandler(IBookingRepository bookingRepository, IClock clock) : IRequestHandler<CancelBookingCommand, ErrorOr<BookingResult>>
    {
        public async Task<ErrorOr<BookingResult>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            // Fetch the booking
            var booking = await bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            // Validate existence
            if (booking is null)
                return BookingErrors.NotFound;
            
            // Validate ownership
            if (booking.UserId != request.UserId)
                return AppBookingErrors.Unauthorized;

            // Validate arrival time
            if (booking.HasArrivalTimePassed(clock.UtcNow))
                return AppBookingErrors.ArrivalTimePassed;

            // Cancel the booking
            var result = booking.Cancel();

            // Handle potential errors from cancellation logic
            if (result.IsError)
                return result.Errors;

            // Persist changes
            await bookingRepository.SaveChangesAsync(cancellationToken);

            // Return the cancelled booking result
            return booking.ToResult();
        }
    }
}
