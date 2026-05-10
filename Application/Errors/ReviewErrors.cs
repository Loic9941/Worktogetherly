using ErrorOr;

namespace WorkTogetherly.Application.Errors
{
    public static class ReviewErrors
    {
        public static Error AlreadyExists => Error.Conflict(
            code: "Review.AlreadyExists",
            description: "Vous avez déjà laissé un avis sur cet espace");

        public static Error NotOwner => Error.Forbidden(
            code: "Review.NotOwner",
            description: "Vous n'êtes pas l'auteur de cet avis");

        public static Error BookingNotPast => Error.Validation(
            code: "Review.BookingNotPast",
            description: "Vous ne pouvez laisser un avis qu'après la fin du créneau");

        public static Error BookingNotOwned => Error.Forbidden(
            code: "Review.BookingNotOwned",
            description: "Cette réservation ne vous appartient pas");
    }
}
