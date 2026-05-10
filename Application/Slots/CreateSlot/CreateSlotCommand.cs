using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Slots.Common;

namespace WorkTogetherly.Application.Slots.CreateSlot
{
    public record CreateSlotCommand(
        int WorkspaceId,
        Guid UserId,
        DateTime StartDateTime,
        DateTime EndDateTime,
        int Capacity) : IRequest<ErrorOr<SlotResult>>;
}
