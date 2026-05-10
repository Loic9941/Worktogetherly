using FluentValidation;

namespace WorkTogetherly.Application.Users.RefreshToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Le refresh token est obligatoire");
        }
    }
}
