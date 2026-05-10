using ErrorOr;

namespace WorkTogetherly.Application.Errors
{
    public static class BookingErrors
    {
        public static Error AlreadyBooked => Error.Conflict(
            code: "Booking.AlreadyBooked",
            description: "Vous avez déjà réservé ce créneau");

        public static Error OwnerCannotBook => Error.Forbidden(
            code: "Booking.OwnerCannotBook",
            description: "Le propriétaire ne peut pas réserver son propre espace");

        public static Error Unauthorized => Error.Forbidden(
            code: "Booking.Unauthorized",
            description: "Vous n'êtes pas autorisé à effectuer cette action sur cette réservation");

        public static Error ArrivalTimePassed => Error.Validation(
            code: "Booking.ArrivalTimePassed",
            description: "Impossible de modifier une réservation dont l'heure d'arrivée est dépassée");
    }
}
