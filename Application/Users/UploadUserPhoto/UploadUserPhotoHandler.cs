using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Users.Common;
using WorkTogetherly.Application.Users.GetCurrentUser;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.UploadUserPhoto
{
    public class UploadUserPhotoHandler(IUserRepository userRepository, IFileService fileService) : IRequestHandler<UploadUserPhotoCommand, ErrorOr<CurrentUserResult>>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IFileService _fileService = fileService;

        public async Task<ErrorOr<CurrentUserResult>> Handle(UploadUserPhotoCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
                return UserErrors.NotFound;

            var photoPath = await _fileService.SaveUserPhotoAsync(
                request.UserId, request.FileStream, request.FileName, cancellationToken);

            var oldPath = user.ReplacePhoto(photoPath);
            if (oldPath is not null)
                await _fileService.DeleteFileAsync(oldPath, cancellationToken);

            await _userRepository.UpdateAsync(user, cancellationToken);

            return user.ToResult();
        }
    }
}
