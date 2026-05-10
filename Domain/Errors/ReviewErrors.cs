using ErrorOr;

namespace WorkTogetherly.Domain.Errors
{
    public static class ReviewErrors
    {
        public static Error NotFound => Error.NotFound(
            code: "Review.NotFound",
            description: "Avis introuvable");

        public static Error InvalidRating => Error.Validation(
            code: "Review.InvalidRating",
            description: "La note doit être comprise entre 1 et 5");
    }
}
