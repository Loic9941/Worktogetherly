using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Slots.Common;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppErrors = WorkTogetherly.Application.Errors;

namespace WorkTogetherly.Application.Slots.CreateSlot
{
    public class CreateSlotHandler(IWorkspaceRepository workspaceRepository, ISlotRepository slotRepository) : IRequestHandler<CreateSlotCommand, ErrorOr<SlotResult>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly ISlotRepository _slotRepository = slotRepository;

        public async Task<ErrorOr<SlotResult>> Handle(CreateSlotCommand request, CancellationToken cancellationToken)
        {
            // Fetch the workspace to validate ownership and existence
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            // Validate that the user is the owner of the workspaces
            if (workspace.UserId != request.UserId)
                return AppErrors.WorkspaceErrors.Unauthorized;

            // Validate that the new slot does not overlap with existing slots in the same workspace
            var existingSlots = await _slotRepository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);
            bool overlaps = existingSlots.Any(s => s.OverlapsWith(request.StartDateTime, request.EndDateTime));
            if (overlaps)
                return AppErrors.SlotErrors.Overlapping;

            // Create the slot and handle potential errors from creation logic
            var slotOrError = Slot.Create(request.WorkspaceId, request.StartDateTime, request.EndDateTime, request.Capacity);
            if (slotOrError.IsError)
                return slotOrError.Errors;

            // Persist the new slot
            var slot = slotOrError.Value;
            await _slotRepository.AddAsync(slot, cancellationToken);
            await _slotRepository.SaveChangesAsync(cancellationToken);

            // Fetch the saved slot to return the result
            var saved = await _slotRepository.GetByIdAsync(slot.Id, cancellationToken);
            return SlotMapper.ToResult(saved!);
        }
    }
}
