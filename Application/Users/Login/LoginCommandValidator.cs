using FluentValidation;

namespace WorkTogetherly.Application.Users.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("L'email est obligatoire")
                .EmailAddress().WithMessage("L'email n'est pas valide");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Le mot de passe est obligatoire");
        }
    }
}
