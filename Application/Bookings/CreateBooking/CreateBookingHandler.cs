using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Domain.Entities;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppBookingErrors = WorkTogetherly.Application.Errors.BookingErrors;
using AppSlotErrors = WorkTogetherly.Application.Errors.SlotErrors;
using AppMaterialErrors = WorkTogetherly.Application.Errors.MaterialErrors;

namespace WorkTogetherly.Application.Bookings.CreateBooking
{
    public class CreateBookingHandler(
        ISlotRepository slotRepository,
        IBookingRepository bookingRepository,
        IWorkspaceRepository workspaceRepository,
        IClock clock) : IRequestHandler<CreateBookingCommand, ErrorOr<int>>
    {
        private readonly ISlotRepository _slotRepository = slotRepository;
        private readonly IBookingRepository _bookingRepository = bookingRepository;
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly IClock _clock = clock;

        public async Task<ErrorOr<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var slot = await _slotRepository.GetByIdAsync(request.SlotId, cancellationToken);

            if (slot is null)
                return SlotErrors.NotFound;

            if (slot.StartDateTime < _clock.UtcNow)
                return AppSlotErrors.InThePast;

            var workspace = await _workspaceRepository.GetByIdAsync(slot.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            if (workspace.UserId == request.UserId)
                return AppBookingErrors.OwnerCannotBook;

            var capacityCheckResult = CheckSlotCapacity(slot, request.UserId);
            if (capacityCheckResult.IsError)
                return capacityCheckResult.Errors;

            var materialCheckResult = await CheckMaterialAvailabilityAsync(slot, workspace, request, cancellationToken);
            if (materialCheckResult.IsError)
                return materialCheckResult.Errors;

            var bookingOrError = Booking.Create(request.SlotId, request.UserId, request.ArrivalTime, slot.StartDateTime, slot.EndDateTime);
            if (bookingOrError.IsError)
                return bookingOrError.Errors;

            var booking = bookingOrError.Value;
            booking.AddMaterials(request.MaterialIds);

            await _bookingRepository.AddAsync(booking, cancellationToken);
            await _bookingRepository.SaveChangesAsync(cancellationToken);

            return booking.Id;
        }

        private static ErrorOr<Success> CheckSlotCapacity(Slot slot, Guid userId)
        {
            if (slot.IsAlreadyBookedBy(userId))
                return AppBookingErrors.AlreadyBooked;

            if (slot.IsFull())
                return AppSlotErrors.SlotFull;

            return Result.Success;
        }

        private async Task<ErrorOr<Success>> CheckMaterialAvailabilityAsync(
            Slot slot, Domain.Entities.Workspace workspace,
            CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // Live DB count per material to catch concurrent bookings that the in-memory slot doesn't know about yet.
            // count >= quantity means no unit is left (>= not > because quantity is the total, not the max index).
            foreach (var materialId in request.MaterialIds)
            {
                var workspaceMaterial = workspace.WorkspaceMaterials.FirstOrDefault(wm => wm.MaterialId == materialId);
                if (workspaceMaterial is not null)
                {
                    var count = await _bookingRepository.CountActiveMaterialBookingsBySlotIdAsync(
                        request.SlotId, materialId, cancellationToken);
                    if (count >= workspaceMaterial.Quantity)
                        return AppMaterialErrors.NotEnoughQuantity;
                }
            }
            return Result.Success;
        }
    }
}
