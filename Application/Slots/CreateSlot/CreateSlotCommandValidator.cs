using FluentValidation;

namespace WorkTogetherly.Application.Slots.CreateSlot
{
    public class CreateSlotCommandValidator : AbstractValidator<CreateSlotCommand>
    {
        public CreateSlotCommandValidator()
        {
            RuleFor(x => x.WorkspaceId).GreaterThan(0);
            RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
            RuleFor(x => x.StartDateTime).LessThan(x => x.EndDateTime)
                .WithMessage("L'heure de début doit être antérieure à l'heure de fin");
        }
    }
}
