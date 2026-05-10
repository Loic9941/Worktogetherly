using System;
using System.Threading;
using System.Threading.Tasks;
using WorkTogetherly.Application.Contracts;
using WorkTogetherly.Application.Repositories;
using WorkTogetherly.Domain.Entities;

namespace WorkTogetherly.Application.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public AccountService(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return new RegisterResult(false, new[] { "Email and password are required." });

        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            return new RegisterResult(false, new[] { "A user with that email already exists." });

        var passwordHash = _hasher.Hash(request.Password);
        var user = User.Create(request.FirstName ?? string.Empty, request.LastName ?? string.Empty, request.Email, passwordHash);

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);

        var dto = new UserDto(user.Id, user.Email, user.FirstName, user.LastName);
        return new RegisterResult(true, Array.Empty<string>(), dto);
    }
}
