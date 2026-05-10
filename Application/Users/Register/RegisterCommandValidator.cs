using FluentValidation;

namespace WorkTogetherly.Application.Users.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("L'email est obligatoire")
                .EmailAddress().WithMessage("L'email n'est pas valide");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Le mot de passe est obligatoire")
                .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Le prénom est obligatoire");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Le nom est obligatoire");
        }
    }
}
