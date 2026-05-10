using FluentValidation;

namespace WorkTogetherly.Application.Slots.GetSlotsByWorkspace
{
    public class GetSlotsByWorkspaceQueryValidator : AbstractValidator<GetSlotsByWorkspaceQuery>
    {
        public GetSlotsByWorkspaceQueryValidator()
        {
            RuleFor(x => x.WorkspaceId).GreaterThan(0);
        }
    }
}
