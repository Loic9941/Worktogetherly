using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Application.Workspaces.CreateWorkspace;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppErrors = WorkTogetherly.Application.Errors;

namespace WorkTogetherly.Application.Workspaces.UploadWorkspacePhoto
{
    public class UploadWorkspacePhotoHandler(IWorkspaceRepository workspaceRepository, IFileService fileService) : IRequestHandler<UploadWorkspacePhotoCommand, ErrorOr<WorkspaceResult>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly IFileService _fileService = fileService;

        public async Task<ErrorOr<WorkspaceResult>> Handle(UploadWorkspacePhotoCommand request, CancellationToken cancellationToken)
        {
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            if (workspace.UserId != request.UserId)
                return AppErrors.WorkspaceErrors.Unauthorized;

            var photoPath = await _fileService.SaveWorkspacePhotoAsync(
                request.WorkspaceId, request.FileStream, request.FileName, cancellationToken);

            var oldPath = workspace.ReplacePhoto(photoPath);
            if (oldPath is not null)
                await _fileService.DeleteFileAsync(oldPath, cancellationToken);

            await _workspaceRepository.SaveChangesAsync(cancellationToken);

            var saved = await _workspaceRepository.GetByIdAsync(workspace.Id, cancellationToken);
            return CreateWorkspaceHandler.MapResult(saved!);
        }
    }
}
