using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Workspaces.Common;

namespace WorkTogetherly.Application.Workspaces.UploadWorkspacePhoto
{
    public record UploadWorkspacePhotoCommand(
        int WorkspaceId,
        Guid UserId,
        Stream FileStream,
        string FileName,
        long FileSizeBytes) : IRequest<ErrorOr<WorkspaceResult>>;
}
