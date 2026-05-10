using FluentValidation;

namespace WorkTogetherly.Application.Users.Logout
{
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Le refresh token est obligatoire");
        }
    }
}
