using ErrorOr;

namespace WorkTogetherly.Domain.Errors
{
    public static class SlotErrors
    {
        public static Error NotFound => Error.NotFound(
            code: "Slot.NotFound",
            description: "Créneau introuvable");

        public static Error AlreadyCancelled => Error.Conflict(
            code: "Slot.AlreadyCancelled",
            description: "Ce créneau est déjà annulé");

        public static Error InvalidTimeRange => Error.Validation(
            code: "Slot.InvalidTimeRange",
            description: "L'heure de début doit être antérieure à l'heure de fin");

        public static Error InvalidCapacity => Error.Validation(
            code: "Slot.InvalidCapacity",
            description: "La capacité doit être supérieure à 0");

        public static Error AlreadyStarted => Error.Conflict(
            code: "Slot.AlreadyStarted",
            description: "Impossible de modifier ou annuler un créneau déjà commencé");
    }
}
