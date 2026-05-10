using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Workspaces.GetWorkspaceDetails;

public static class SlotAvailabilityCalculator
{
    public static int AvailablePlaces(Slot slot)
    {
        var active = slot.Bookings.Count(b => !b.IsCancelled);
        return slot.Capacity - active;
    }

    public static int AvailableMaterialQuantity(Slot slot, WorkspaceMaterial workspaceMaterial)
    {
        var booked = slot.Bookings
            .Where(b => !b.IsCancelled)
            .SelectMany(b => b.BookingMaterials)
            .Count(bm => bm.MaterialId == workspaceMaterial.MaterialId);
        return Math.Max(0, workspaceMaterial.Quantity - booked);
    }

    public static double AverageRating(IReadOnlyList<Review> reviews) =>
        reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0.0;
}
