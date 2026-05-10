using Microsoft.AspNetCore.Hosting;
using WorkTogetherly.Application.Interfaces;

namespace WorkTogetherly.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _webRootPath;

        public FileService(IWebHostEnvironment env)
        {
            _webRootPath = env.WebRootPath;
        }

        public async Task<string> SaveWorkspacePhotoAsync(
            int workspaceId,
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var relativeDir = Path.Combine("uploads", "workspaces", workspaceId.ToString());
            var absoluteDir = Path.Combine(_webRootPath, relativeDir);

            Directory.CreateDirectory(absoluteDir);

            // Delete any previous photo file (handles extension change: jpg → png etc.)
            foreach (var old in Directory.GetFiles(absoluteDir, "photo.*"))
                File.Delete(old);

            var relativeFilePath = Path.Combine(relativeDir, $"photo{ext}").Replace('\\', '/');
            var absoluteFilePath = Path.Combine(_webRootPath,
                relativeFilePath.Replace('/', Path.DirectorySeparatorChar));

            await using var fs = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fs, cancellationToken);

            return "/" + relativeFilePath;
        }

        public async Task<string> SaveUserPhotoAsync(
            Guid userId,
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            var relativeDir = Path.Combine("uploads", "users", userId.ToString());
            var absoluteDir = Path.Combine(_webRootPath, relativeDir);

            Directory.CreateDirectory(absoluteDir);

            foreach (var old in Directory.GetFiles(absoluteDir, "photo.*"))
                File.Delete(old);

            var relativeFilePath = Path.Combine(relativeDir, $"photo{ext}").Replace('\\', '/');
            var absoluteFilePath = Path.Combine(_webRootPath,
                relativeFilePath.Replace('/', Path.DirectorySeparatorChar));

            await using var fs = new FileStream(absoluteFilePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fs, cancellationToken);

            return "/" + relativeFilePath;
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;

            var absolutePath = Path.Combine(_webRootPath,
                relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(absolutePath))
                File.Delete(absolutePath);

            return Task.CompletedTask;
        }
    }
}
