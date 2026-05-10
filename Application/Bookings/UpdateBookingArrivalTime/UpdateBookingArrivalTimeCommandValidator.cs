using FluentValidation;

namespace WorkTogetherly.Application.Bookings.UpdateBookingArrivalTime
{
    public class UpdateBookingArrivalTimeCommandValidator : AbstractValidator<UpdateBookingArrivalTimeCommand>
    {
        public UpdateBookingArrivalTimeCommandValidator()
        {
            RuleFor(x => x.BookingId).GreaterThan(0);
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
