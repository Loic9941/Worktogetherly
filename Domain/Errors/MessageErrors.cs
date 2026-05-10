using ErrorOr;

namespace WorkTogetherly.Domain.Errors
{
    public static class MessageErrors
    {
        public static Error NotFound => Error.NotFound(
            code: "Message.NotFound",
            description: "Message introuvable");
    }
}
