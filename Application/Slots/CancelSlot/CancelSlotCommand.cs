using ErrorOr;
using MediatR;

namespace WorkTogetherly.Application.Slots.CancelSlot
{
    public record CancelSlotCommand(int SlotId, int WorkspaceId, Guid UserId) : IRequest<ErrorOr<Deleted>>;
}
