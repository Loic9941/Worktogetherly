using FluentValidation;

namespace WorkTogetherly.Application.Users.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Le prénom est obligatoire");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Le nom est obligatoire");
        }
    }
}
