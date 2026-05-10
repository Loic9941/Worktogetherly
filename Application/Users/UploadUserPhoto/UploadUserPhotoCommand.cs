using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Users.GetCurrentUser;

namespace WorkTogetherly.Application.Users.UploadUserPhoto
{
    public record UploadUserPhotoCommand(
        Guid UserId,
        Stream FileStream,
        string FileName,
        long FileSizeBytes
    ) : IRequest<ErrorOr<CurrentUserResult>>;
}
