using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Workspaces.Common;
using WorkTogetherly.Application.Workspaces.CreateWorkspace;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;
using AppErrors = WorkTogetherly.Application.Errors;

namespace WorkTogetherly.Application.Workspaces.DeleteWorkspacePhoto
{
    public class DeleteWorkspacePhotoHandler(IWorkspaceRepository workspaceRepository, IFileService fileService) : IRequestHandler<DeleteWorkspacePhotoCommand, ErrorOr<WorkspaceResult>>
    {
        private readonly IWorkspaceRepository _workspaceRepository = workspaceRepository;
        private readonly IFileService _fileService = fileService;

        public async Task<ErrorOr<WorkspaceResult>> Handle(DeleteWorkspacePhotoCommand request, CancellationToken cancellationToken)
        {
            // Fetch the workspace by ID
            var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId, cancellationToken);
            
            // Check if the workspace exists
            if (workspace is null)
                return WorkspaceErrors.NotFound;

            // Check if the user is authorized to delete the photo
            if (workspace.UserId != request.UserId)
                return AppErrors.WorkspaceErrors.Unauthorized;

            // Remove the photo and delete the old file if it exists
            var oldPath = workspace.RemovePhoto();
            if (oldPath is not null)
                await _fileService.DeleteFileAsync(oldPath, cancellationToken);

            // Persist changes
            await _workspaceRepository.SaveChangesAsync(cancellationToken);

            // Fetch the updated workspace and return the result
            var saved = await _workspaceRepository.GetByIdAsync(workspace.Id, cancellationToken);
            return CreateWorkspaceHandler.MapResult(saved!);
        }
    }
}
