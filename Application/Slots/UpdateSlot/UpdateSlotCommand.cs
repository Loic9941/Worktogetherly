using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Slots.Common;

namespace WorkTogetherly.Application.Slots.UpdateSlot
{
    public record UpdateSlotCommand(
        int SlotId,
        int WorkspaceId,
        Guid UserId,
        DateTime StartDateTime,
        DateTime EndDateTime,
        int Capacity) : IRequest<ErrorOr<SlotResult>>;
}
