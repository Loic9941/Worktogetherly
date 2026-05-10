using FluentValidation;

namespace WorkTogetherly.Application.Bookings.CreateBooking
{
    public class CreateBookingCommandValidator : AbstractValidator<CreateBookingCommand>
    {
        public CreateBookingCommandValidator()
        {
            RuleFor(x => x.SlotId).GreaterThan(0);
            RuleFor(x => x.ArrivalTime).NotEqual(default(TimeOnly));
        }
    }
}
