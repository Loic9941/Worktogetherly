using ErrorOr;
using MediatR;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Users.Common;
using WorkTogetherly.Application.Users.GetCurrentUser;
using WorkTogetherly.Domain.Errors;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.DeleteUserPhoto
{
    public class DeleteUserPhotoHandler(IUserRepository userRepository, IFileService fileService) : IRequestHandler<DeleteUserPhotoCommand, ErrorOr<CurrentUserResult>>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IFileService _fileService = fileService;

        public async Task<ErrorOr<CurrentUserResult>> Handle(DeleteUserPhotoCommand request, CancellationToken cancellationToken)
        {
            // Fetch the user
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            // Validate user existence
            if (user is null)
                return UserErrors.NotFound;

            // Remove the photo and delete the old file if it exists
            var oldPath = user.RemovePhoto();
            if (oldPath is not null)
                await _fileService.DeleteFileAsync(oldPath, cancellationToken);

            // Persist changes
            await _userRepository.UpdateAsync(user, cancellationToken);
            
            return user.ToResult();
        }
    }
}
