using ErrorOr;

namespace WorkTogetherly.Domain.Errors
{
    public static class BookingErrors
    {
        public static Error NotFound => Error.NotFound(
            code: "Booking.NotFound",
            description: "Réservation introuvable");

        public static Error AlreadyCancelled => Error.Conflict(
            code: "Booking.AlreadyCancelled",
            description: "La réservation est déjà annulée");

        public static Error ArrivalTimeOutOfRange => Error.Validation(
            code: "Booking.ArrivalTimeOutOfRange",
            description: "L'heure d'arrivée doit être dans la plage du créneau");
    }
}
