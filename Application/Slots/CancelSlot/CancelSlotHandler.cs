using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppErrors = WorkTogetherly.Application.Errors;

namespace WorkTogetherly.Application.Slots.CancelSlot
{
    public class CancelSlotHandler(IWorkspaceRepository workspaceRepository, ISlotRepository slotRepository, IClock clock) : IRequestHandler<CancelSlotCommand, ErrorOr<Deleted>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly ISlotRepository _slotRepository = slotRepository;
        private readonly IClock _clock = clock;

        public async Task<ErrorOr<Deleted>> Handle(CancelSlotCommand request, CancellationToken cancellationToken)
        {
            var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
            if (slot is null)
                return SlotErrors.NotFound;

            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            if (workspace.UserId != request.UserId)
                return AppErrors.WorkspaceErrors.Unauthorized;

            var cancelResult = slot.Cancel(_clock.UtcNow);
            if (cancelResult.IsError)
                return cancelResult.Errors;

            await _slotRepository.UpdateAsync(slot, cancellationToken);
            await _slotRepository.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
