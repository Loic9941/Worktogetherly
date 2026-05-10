using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Common.Services;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Workspaces.GetWorkspaceDetails
{
    public class GetWorkspaceDetailsHandler(
        IWorkspaceRepository workspaceRepository,
        ISlotRepository slotRepository,
        IReviewRepository reviewRepository,
        IClock clock) : IRequestHandler<GetWorkspaceDetailsQuery, ErrorOr<WorkspaceDetailsResult>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly ISlotRepository _slotRepository = slotRepository;
        private readonly IReviewRepository _reviewRepository = reviewRepository;
        private readonly IClock _clock = clock;

        public async Task<ErrorOr<WorkspaceDetailsResult>> Handle(GetWorkspaceDetailsQuery request, CancellationToken cancellationToken)
        {
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            var targetDate = request.Date.ToDateTime(TimeOnly.MinValue).Date;
            var allSlots = await _slotRepository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);
            var slotsForDate = allSlots.Where(s => s.StartDateTime.Date == targetDate).ToList();

            var reviews = await _reviewRepository.GetByWorkspaceIdAsync(request.WorkspaceId, cancellationToken);
            var reviewCount = reviews.Count;
            var averageRating = SlotAvailabilityCalculator.AverageRating(reviews);

            var isOwner = workspace.UserId == request.UserId;
            var now = _clock.UtcNow;

            var workspaceMaterials = workspace.WorkspaceMaterials.ToList();

            var slotResults = slotsForDate.Select(s =>
            {
                var availablePlaces = SlotAvailabilityCalculator.AvailablePlaces(s);
                var userHasBooked = s.Bookings.Any(b => !b.IsCancelled && b.UserId == request.UserId);
                var isInPast = s.StartDateTime < now;

                var materialAvailabilities = workspaceMaterials.Select(wm =>
                {
                    var available = SlotAvailabilityCalculator.AvailableMaterialQuantity(s, wm);
                    return new MaterialAvailabilityResult(wm.MaterialId, wm.Material.Name, wm.Quantity, available);
                }).ToList();

                return new SlotDetailResult(s.Id, s.StartDateTime, s.EndDateTime, s.Capacity, availablePlaces, userHasBooked, isInPast, materialAvailabilities);
            }).ToList();

            var materials = workspace.WorkspaceMaterials
                .Select(wm => new WorkspaceMaterialDetailResult(wm.MaterialId, wm.Material.Name, wm.Quantity))
                .ToList();

            var rules = workspace.WorkspaceRules
                .Select(wr => new WorkspaceRuleDetailResult(wr.RuleId, wr.Rule.Name))
                .ToList();

            var (approxLat, approxLng) = GeoCalculator.ApproximateCoords(workspace.Id, workspace.Latitude, workspace.Longitude);

            return new WorkspaceDetailsResult(
                workspace.Id,
                workspace.Name,
                workspace.Description,
                approxLat,
                approxLng,
                averageRating,
                reviewCount,
                isOwner,
                materials,
                rules,
                slotResults);
        }
    }
}
