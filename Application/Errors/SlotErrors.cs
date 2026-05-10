using ErrorOr;

namespace WorkTogetherly.Application.Errors
{
    public static class SlotErrors
    {
        public static Error InThePast => Error.Validation(
            code: "Slot.InThePast",
            description: "Impossible de réserver un créneau passé");

        public static Error SlotFull => Error.Conflict(
            code: "Slot.Full",
            description: "Ce créneau est complet");

        public static Error Overlapping => Error.Conflict(
            code: "Slot.Overlapping",
            description: "Ce créneau chevauche un créneau existant sur ce workspace");
    }
}
