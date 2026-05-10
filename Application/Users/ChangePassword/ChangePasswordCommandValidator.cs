using FluentValidation;

namespace WorkTogetherly.Application.Users.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Le mot de passe actuel est obligatoire");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Le nouveau mot de passe est obligatoire")
                .MinimumLength(8).WithMessage("Le mot de passe doit contenir au moins 8 caractères");
        }
    }
}
