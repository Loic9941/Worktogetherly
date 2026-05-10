using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Bookings.Common;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppBookingErrors = WorkTogetherly.Application.Errors.BookingErrors;

namespace WorkTogetherly.Application.Bookings.UpdateBookingArrivalTime
{
    public class UpdateBookingArrivalTimeHandler(IBookingRepository bookingRepository, IClock clock) : IRequestHandler<UpdateBookingArrivalTimeCommand, ErrorOr<BookingResult>>
    {
        private readonly IBookingRepository _bookingRepository = bookingRepository;
        private readonly IClock _clock = clock;

        public async Task<ErrorOr<BookingResult>> Handle(UpdateBookingArrivalTimeCommand request, CancellationToken cancellationToken)
        {
            // Fetch the booking
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken);
            if (booking is null)
                return BookingErrors.NotFound;

            // Validate ownership
            if (booking.UserId != request.UserId)
                return AppBookingErrors.Unauthorized;

            // Validate arrival time
            if (booking.HasArrivalTimePassed(_clock.UtcNow))
                return AppBookingErrors.ArrivalTimePassed;

            // Update the arrival time
            var updateResult = booking.UpdateArrivalTime(
                request.NewArrivalTime,
                booking.Slot.StartDateTime,
                booking.Slot.EndDateTime);

            // Handle potential errors from update logic
            if (updateResult.IsError)
                return updateResult.Errors;

            // Persist changes
            await _bookingRepository.SaveChangesAsync(cancellationToken);

            return booking.ToResult();
        }
    }
}
