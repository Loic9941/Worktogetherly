namespace WorkTogetherly.Shared.Models
{
    public record ReviewDto(
        int Id,
        int BookingId,
        int WorkspaceId,
        int Rating,
        string Comment,
        DateTime CreatedAt,
        string? ReviewerName);

    public record CreateReviewRequest(int BookingId, int Rating, string Comment);

    public record UpdateReviewRequest(int Rating, string Comment);
}
