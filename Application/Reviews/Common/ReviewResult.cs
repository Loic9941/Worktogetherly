namespace WorkTogetherly.Application.Reviews.Common
{
    public record ReviewResult(
        int Id,
        int BookingId,
        int WorkspaceId,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        string? ReviewerName);
}
