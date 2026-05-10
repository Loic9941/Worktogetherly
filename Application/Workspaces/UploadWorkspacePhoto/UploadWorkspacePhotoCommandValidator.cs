using FluentValidation;

namespace WorkTogetherly.Application.Workspaces.UploadWorkspacePhoto
{
    public class UploadWorkspacePhotoCommandValidator : AbstractValidator<UploadWorkspacePhotoCommand>
    {
        private const long MaxBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

        public UploadWorkspacePhotoCommandValidator()
        {
            RuleFor(x => x.FileSizeBytes)
                .LessThanOrEqualTo(MaxBytes)
                .WithMessage("La photo ne peut pas dépasser 5 Mo");

            RuleFor(x => x.FileName)
                .Must(name => AllowedExtensions.Contains(Path.GetExtension(name)))
                .WithMessage("Format non supporté. Formats acceptés : .jpg, .jpeg, .png, .webp");
        }
    }
}
