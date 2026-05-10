using FluentValidation;

namespace WorkTogetherly.Application.Workspaces.CreateWorkspace
{
    public class CreateWorkspaceCommandValidator : AbstractValidator<CreateWorkspaceCommand>
    {
        public CreateWorkspaceCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom est obligatoire")
                .MaximumLength(200).WithMessage("Le nom ne peut pas dépasser 200 caractères");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La description est obligatoire")
                .MaximumLength(2000).WithMessage("La description ne peut pas dépasser 2000 caractères");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("L'adresse est obligatoire")
                .MaximumLength(500).WithMessage("L'adresse ne peut pas dépasser 500 caractères");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("La latitude doit être comprise entre -90 et 90");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("La longitude doit être comprise entre -180 et 180");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("La capacité doit être supérieure à 0");

            RuleForEach(x => x.Materials).ChildRules(m =>
            {
                m.RuleFor(item => item.MaterialId)
                    .GreaterThan(0).WithMessage("L'identifiant du matériel est invalide");
                m.RuleFor(item => item.Quantity)
                    .GreaterThan(0).WithMessage("La quantité doit être supérieure à 0");
            });

            RuleForEach(x => x.RuleIds)
                .GreaterThan(0).WithMessage("L'identifiant de la règle est invalide");
        }
    }
}
