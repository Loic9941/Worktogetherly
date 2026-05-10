namespace WorkTogetherly.Presentation.Models.Review
{
    public record CreateReviewRequest(int BookingId, int Rating, string Comment);
}
