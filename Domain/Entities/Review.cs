using ErrorOr;
using WorkTogetherly.Domain.Errors;

namespace WorkTogetherly.Domain.Entities
{
    public class Review
    {
        public int Id { get; private set; }
        public int BookingId { get; private set; }
        public Guid ReviewerId { get; private set; }
        public int WorkspaceId { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Booking Booking { get; private set; } = null!;
        public User Reviewer { get; private set; } = null!;
        public Workspace Workspace { get; private set; } = null!;

        private Review() { }

        public static ErrorOr<Review> Create(int bookingId, Guid reviewerId, int workspaceId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                return ReviewErrors.InvalidRating;

            return new Review
            {
                BookingId = bookingId,
                ReviewerId = reviewerId,
                WorkspaceId = workspaceId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };
        }

        public ErrorOr<Success> Update(int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                return ReviewErrors.InvalidRating;

            Rating = rating;
            Comment = comment;
            return Result.Success;
        }
    }
}
