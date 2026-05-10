using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Slots.Common;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppErrors = WorkTogetherly.Application.Errors;

namespace WorkTogetherly.Application.Slots.UpdateSlot
{
    public class UpdateSlotHandler(IWorkspaceRepository workspaceRepository, ISlotRepository slotRepository, IClock clock) : IRequestHandler<UpdateSlotCommand, ErrorOr<SlotResult>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly ISlotRepository _slotRepository = slotRepository;
        private readonly IClock _clock = clock;

        public async Task<ErrorOr<SlotResult>> Handle(UpdateSlotCommand request, CancellationToken cancellationToken)
        {
            // Fetch the slot to update
            var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);
            
            // Validate slot existence
            if (slot is null)
                return SlotErrors.NotFound;

            // Fetch the workspace to validate ownership and existence
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            // Validate that the user is the owner of the workspaces
            if (workspace.UserId != request.UserId)
                return AppErrors.WorkspaceErrors.Unauthorized;

            // Validate that the new slot does not overlap with existing slots in the same workspace
            var existingSlots = await _slotRepository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);
            bool overlaps = existingSlots.Any(s =>
                s.Id != request.SlotId && s.OverlapsWith(request.StartDateTime, request.EndDateTime));
            if (overlaps)
                return AppErrors.SlotErrors.Overlapping;

            // Validate that the slot has not already started
            if (slot.HasStarted(_clock.UtcNow))
                return AppErrors.SlotErrors.AlreadyStarted;

            // Update the slot and handle potential errors from update logic
            var updateResult = slot.Update(request.StartDateTime, request.EndDateTime, request.Capacity);
            if (updateResult.IsError)
                return updateResult.Errors;

            // Persist changes
            await _slotRepository.UpdateAsync(slot, cancellationToken);
            await _slotRepository.SaveChangesAsync(cancellationToken);

            // Fetch the updated slot to return the result
            var saved = await _slotRepository.GetByIdAsync(slot.Id, cancellationToken);
            return SlotMapper.ToResult(saved!);
        }
    }
}
