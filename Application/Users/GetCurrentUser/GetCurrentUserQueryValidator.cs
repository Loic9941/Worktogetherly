using FluentValidation;

namespace WorkTogetherly.Application.Users.GetCurrentUser
{
    public class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery>
    {
        public GetCurrentUserQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty).WithMessage("L'identifiant utilisateur est invalide");
        }
    }
}
