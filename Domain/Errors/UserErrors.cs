using ErrorOr;

namespace WorkTogetherly.Domain.Errors
{
    public static class UserErrors
    {
        public static Error NotFound => Error.NotFound(
            code: "User.NotFound",
            description: "Utilisateur introuvable");
    }
}
