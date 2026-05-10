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
            // Fetch the slot by ID
            var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
            if (slot is null)
                return SlotErrors.NotFound;

            // Fetch the workspace to validate ownership
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            // Validate that the user is the owner of the workspaces
            if (workspace.UserId != request.UserId)
                return AppErrors.WorkspaceErrors.Unauthorized;

            // Validate that the slot has not already started
            if (slot.HasStarted(_clock.UtcNow))
                return AppErrors.SlotErrors.AlreadyStarted;

            // Cancel the slot and handle potential errors from cancellation logic
            var cancelResult = slot.Cancel();
            if (cancelResult.IsError)
                return cancelResult.Errors;

            // Persist changes
            await _slotRepository.UpdateAsync(slot, cancellationToken);
            await _slotRepository.SaveChangesAsync(cancellationToken);

            // Return a deleted result to indicate successful cancellation
            return Result.Deleted;
        }
    }
}
