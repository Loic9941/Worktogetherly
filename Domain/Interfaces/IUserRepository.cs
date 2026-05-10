using ErrorOr;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<ErrorOr<User>> CreateWithPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> ValidatePasswordAsync(User user, string password);
    Task<ErrorOr<Updated>> ChangePasswordAsync(User user, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<ErrorOr<Success>> ResetPasswordAsync(User user, string token, string newPassword);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
