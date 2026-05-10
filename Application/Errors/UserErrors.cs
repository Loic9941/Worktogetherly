using ErrorOr;

namespace WorkTogetherly.Application.Errors
{
    public static class UserErrors
    {
        public static Error EmailAlreadyExists => Error.Conflict(
            code: "User.EmailAlreadyExists",
            description: "Un compte existe déjà avec cet email");

        public static Error InvalidCredentials => Error.Unauthorized(
            code: "User.InvalidCredentials",
            description: "Email ou mot de passe incorrect");

        public static Error InvalidRefreshToken => Error.Unauthorized(
            code: "User.InvalidRefreshToken",
            description: "Le refresh token est invalide ou expiré");

        public static Error PhotoTooLarge => Error.Validation(
            code: "User.PhotoTooLarge",
            description: "La photo ne doit pas dépasser 5 Mo");

        public static Error PhotoInvalidFormat => Error.Validation(
            code: "User.PhotoInvalidFormat",
            description: "Format non supporté. Utilisez .jpg, .jpeg, .png ou .webp");
    }
}
