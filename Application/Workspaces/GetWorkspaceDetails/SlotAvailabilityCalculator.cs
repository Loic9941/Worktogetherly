using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Workspaces.GetWorkspaceDetails;

public static class SlotAvailabilityCalculator
{
    public static double AverageRating(IReadOnlyList<Review> reviews) =>
        reviews.Count > 0 ? reviews.Average(r => r.Rating) : 0.0;
}
