using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Slots.Common;

namespace WorkTogetherly.Application.Slots.GetSlotsByWorkspace
{
    public record GetSlotsByWorkspaceQuery(int WorkspaceId)
        : IRequest<ErrorOr<IReadOnlyList<SlotResult>>>;
}
