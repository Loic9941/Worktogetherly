using FluentValidation;

namespace WorkTogetherly.Application.Workspaces.Search
{
    public class SearchWorkspacesQueryValidator : AbstractValidator<SearchWorkspacesQuery>
    {
        public SearchWorkspacesQueryValidator()
        {
            RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
            RuleFor(x => x.RadiusKm).InclusiveBetween(1, 200);
            RuleFor(x => x.Date)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("La date doit être aujourd'hui ou dans le futur.");
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}
