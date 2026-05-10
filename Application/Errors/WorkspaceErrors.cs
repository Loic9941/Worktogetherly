using ErrorOr;

namespace WorkTogetherly.Application.Errors
{
    public static class WorkspaceErrors
    {
        public static Error Unauthorized => Error.Forbidden(
            code: "Workspace.Unauthorized",
            description: "Vous n'êtes pas autorisé à modifier cet espace de travail");

        public static Error PhotoTooLarge => Error.Validation(
            code: "Workspace.PhotoTooLarge",
            description: "La photo ne doit pas dépasser 5 Mo");

        public static Error PhotoInvalidFormat => Error.Validation(
            code: "Workspace.PhotoInvalidFormat",
            description: "Seuls les formats JPG, JPEG, PNG et WEBP sont acceptés");


    }
}
