using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Slots.Common;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Slots.GetSlotsByWorkspace
{
    public class GetSlotsByWorkspaceHandler(IWorkspaceRepository workspaceRepository, ISlotRepository slotRepository, IClock clock)
                : IRequestHandler<GetSlotsByWorkspaceQuery, ErrorOr<IReadOnlyList<SlotResult>>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly ISlotRepository _slotRepository = slotRepository;
        private readonly IClock _clock = clock;

        public async Task<ErrorOr<IReadOnlyList<SlotResult>>> Handle(
            GetSlotsByWorkspaceQuery request,
            CancellationToken cancellationToken)
        {
            // Fetch the workspace
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            
            // Validate workspace existence
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            // fetch slots for the workspace
            var slots = await _slotRepository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);
            var now = _clock.UtcNow;

            // Filter out past and cancelled slots, then map to results
            var results = slots
                .Where(s => !s.HasStarted(now) && !s.IsCancelled)
                .Select(SlotMapper.ToResult)
                .ToList();

            return results;
        }
    }
}
