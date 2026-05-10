using ErrorOr;

namespace WorkTogetherly.Application.Errors;

public static class MaterialErrors
{
    public static readonly Error NotEnoughQuantity = Error.Conflict(
        "Material.NotEnoughQuantity",
        "La quantité disponible pour ce matériel est insuffisante sur ce créneau.");
}
