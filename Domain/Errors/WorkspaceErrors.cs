using ErrorOr;

namespace WorkTogetherly.Domain.Errors
{
    public static class WorkspaceErrors
    {
        public static Error NotFound => Error.NotFound(
            code: "Workspace.NotFound",
            description: "Espace de travail introuvable");
    }
}
