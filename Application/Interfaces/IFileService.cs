namespace WorkTogetherly.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveWorkspacePhotoAsync(int workspaceId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
        Task<string> SaveUserPhotoAsync(Guid userId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
    }
}
