using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using TestWorkTogetherly.Helpers;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Settings;
using WorkTogetherly.Application.Users.ChangePassword;
using WorkTogetherly.Application.Users.DeleteUserPhoto;
using WorkTogetherly.Application.Users.ForgotPassword;
using WorkTogetherly.Application.Users.GetCurrentUser;
using WorkTogetherly.Application.Users.Login;
using WorkTogetherly.Application.Users.Logout;
using WorkTogetherly.Application.Users.RefreshToken;
using WorkTogetherly.Application.Users.Register;
using WorkTogetherly.Application.Users.ResetPassword;
using WorkTogetherly.Application.Users.Shared;
using WorkTogetherly.Application.Users.UpdateUser;
using WorkTogetherly.Application.Users.UploadUserPhoto;
using WorkTogetherly.Domain.Interfaces;
using DomainUserErrors = WorkTogetherly.Domain.Errors.UserErrors;
using AppUserErrors = WorkTogetherly.Application.Errors.UserErrors;
using DomainUser = WorkTogetherly.Domain.Entities.User;
using WorkTogetherly.Domain.Entities;

namespace TestWorkTogetherly.Application.User;

public class UserHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly AuthResult SampleAuth = new("access", "refresh", DateTime.UtcNow.AddHours(1));

    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IFileService _fileService = Substitute.For<IFileService>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();

    private static DomainUser MakeUser(string email = "user@test.com")
    {
        var user = DomainUser.Create("Alice", "Dupont", email);
        typeof(DomainUser).GetProperty("Id")!.SetValue(user, UserId);
        return user;
    }

    // ── LoginHandler ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WhenUserNotFound_ReturnsInvalidCredentials()
    {
        _userRepo.GetByEmailAsync("x@x.com", default).Returns((DomainUser?)null);
        var handler = new LoginHandler(_userRepo, _tokenService);

        var result = await handler.Handle(new LoginCommand("x@x.com", "pass"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppUserErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task Login_WhenPasswordInvalid_ReturnsInvalidCredentials()
    {
        var user = MakeUser();
        _userRepo.GetByEmailAsync(user.Email!, default).Returns(user);
        _userRepo.ValidatePasswordAsync(user, "wrong").Returns(false);
        var handler = new LoginHandler(_userRepo, _tokenService);

        var result = await handler.Handle(new LoginCommand(user.Email!, "wrong"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppUserErrors.InvalidCredentials.Code);
    }

    [Fact]
    public async Task Login_WhenValid_ReturnsAuthResult()
    {
        var user = MakeUser();
        _userRepo.GetByEmailAsync(user.Email!, default).Returns(user);
        _userRepo.ValidatePasswordAsync(user, "pass").Returns(true);
        _tokenService.GenerateTokensAsync(user).Returns(SampleAuth);
        var handler = new LoginHandler(_userRepo, _tokenService);

        var result = await handler.Handle(new LoginCommand(user.Email!, "pass"), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(SampleAuth);
    }

    // ── RegisterHandler ───────────────────────────────────────────────────────

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ReturnsEmailAlreadyExists()
    {
        var existing = MakeUser();
        _userRepo.GetByEmailAsync(existing.Email!, default).Returns(existing);
        var handler = new RegisterHandler(_userRepo, _tokenService);

        var result = await handler.Handle(new RegisterCommand(existing.Email!, "pass", "A", "B"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(AppUserErrors.EmailAlreadyExists.Code);
    }

    [Fact]
    public async Task Register_WhenValid_CreatesUserAndReturnsAuthResult()
    {
        _userRepo.GetByEmailAsync("new@test.com", default).Returns((DomainUser?)null);
        var newUser = MakeUser("new@test.com");
        _userRepo.CreateWithPasswordAsync(Arg.Any<DomainUser>(), "pass", default)
            .Returns(newUser);
        _tokenService.GenerateTokensAsync(newUser).Returns(SampleAuth);
        var handler = new RegisterHandler(_userRepo, _tokenService);

        var result = await handler.Handle(new RegisterCommand("new@test.com", "pass", "A", "B"), default);

        result.IsError.Should().BeFalse();
    }

    // ── GetCurrentUserHandler ─────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentUser_WhenNotFound_ReturnsNotFound()
    {
        _userRepo.GetByIdAsync(UserId, default).Returns((DomainUser?)null);
        var handler = new GetCurrentUserHandler(_userRepo);

        var result = await handler.Handle(new GetCurrentUserQuery(UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainUserErrors.NotFound.Code);
    }

    [Fact]
    public async Task GetCurrentUser_WhenFound_ReturnsResult()
    {
        var user = MakeUser();
        _userRepo.GetByIdAsync(UserId, default).Returns(user);
        var handler = new GetCurrentUserHandler(_userRepo);

        var result = await handler.Handle(new GetCurrentUserQuery(UserId), default);

        result.IsError.Should().BeFalse();
        result.Value.Email.Should().Be(user.Email);
    }

    // ── UpdateUserHandler ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUser_WhenNotFound_ReturnsNotFound()
    {
        _userRepo.GetByIdAsync(UserId, default).Returns((DomainUser?)null);
        var handler = new UpdateUserHandler(_userRepo);

        var result = await handler.Handle(new UpdateUserCommand(UserId, "Bob", "Martin"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainUserErrors.NotFound.Code);
    }

    [Fact]
    public async Task UpdateUser_WhenValid_UpdatesAndSaves()
    {
        var user = MakeUser();
        _userRepo.GetByIdAsync(UserId, default).Returns(user);
        var handler = new UpdateUserHandler(_userRepo);

        var result = await handler.Handle(new UpdateUserCommand(UserId, "Bob", "Martin"), default);

        result.IsError.Should().BeFalse();
        user.FirstName.Should().Be("Bob");
        await _userRepo.Received(1).UpdateAsync(user, default);
    }

    // ── ChangePasswordHandler ─────────────────────────────────────────────────

    [Fact]
    public async Task ChangePassword_WhenNotFound_ReturnsNotFound()
    {
        _userRepo.GetByIdAsync(UserId, default).Returns((DomainUser?)null);
        var handler = new ChangePasswordHandler(_userRepo);

        var result = await handler.Handle(new ChangePasswordCommand(UserId, "old", "new"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainUserErrors.NotFound.Code);
    }

    [Fact]
    public async Task ChangePassword_WhenValid_DelegatesToRepository()
    {
        var user = MakeUser();
        _userRepo.GetByIdAsync(UserId, default).Returns(user);
        _userRepo.ChangePasswordAsync(user, "old", "new").Returns(Result.Updated);
        var handler = new ChangePasswordHandler(_userRepo);

        var result = await handler.Handle(new ChangePasswordCommand(UserId, "old", "new"), default);

        result.IsError.Should().BeFalse();
        await _userRepo.Received(1).ChangePasswordAsync(user, "old", "new");
    }

    // ── RefreshTokenHandler ───────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_WhenValid_DelegatesToTokenService()
    {
        _tokenService.RefreshTokenAsync("refresh-token").Returns(SampleAuth);
        var handler = new RefreshTokenHandler(_tokenService);

        var result = await handler.Handle(new RefreshTokenCommand("refresh-token"), default);

        result.IsError.Should().BeFalse();
        result.Value.Should().Be(SampleAuth);
    }

    // ── LogoutHandler ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WhenCalled_DelegatesToTokenService()
    {
        _tokenService.RevokeTokenAsync("refresh-token").Returns(true);
        var handler = new LogoutHandler(_tokenService);

        var result = await handler.Handle(new LogoutCommand("refresh-token"), default);

        result.IsError.Should().BeFalse();
        await _tokenService.Received(1).RevokeTokenAsync("refresh-token");
    }

    // ── ForgotPasswordHandler ─────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPassword_WhenUserNotFound_ReturnsSuccessSilently()
    {
        _userRepo.GetByEmailAsync("x@x.com", default).Returns((DomainUser?)null);
        var handler = new ForgotPasswordHandler(_userRepo, _emailService, Options.Create(new FrontendSettings()));

        var result = await handler.Handle(new ForgotPasswordCommand("x@x.com"), default);

        result.IsError.Should().BeFalse();
        await _emailService.DidNotReceive().SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ForgotPassword_WhenUserFound_SendsEmail()
    {
        var user = MakeUser();
        _userRepo.GetByEmailAsync(user.Email!, default).Returns(user);
        _userRepo.GeneratePasswordResetTokenAsync(user).Returns("reset-token");
        var handler = new ForgotPasswordHandler(_userRepo, _emailService, Options.Create(new FrontendSettings()));

        var result = await handler.Handle(new ForgotPasswordCommand(user.Email!), default);

        result.IsError.Should().BeFalse();
        await _emailService.Received(1).SendAsync(user.Email!, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── ResetPasswordHandler ──────────────────────────────────────────────────

    [Fact]
    public async Task ResetPassword_WhenUserNotFound_ReturnsNotFound()
    {
        _userRepo.GetByEmailAsync("x@x.com", default).Returns((DomainUser?)null);
        var handler = new ResetPasswordHandler(_userRepo);

        var result = await handler.Handle(new ResetPasswordCommand("x@x.com", "token", "newpass"), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainUserErrors.NotFound.Code);
    }

    [Fact]
    public async Task ResetPassword_WhenValid_DelegatesToRepository()
    {
        var user = MakeUser();
        _userRepo.GetByEmailAsync(user.Email!, default).Returns(user);
        _userRepo.ResetPasswordAsync(user, "token", "newpass").Returns(Result.Success);
        var handler = new ResetPasswordHandler(_userRepo);

        var result = await handler.Handle(new ResetPasswordCommand(user.Email!, "token", "newpass"), default);

        result.IsError.Should().BeFalse();
        await _userRepo.Received(1).ResetPasswordAsync(user, "token", "newpass");
    }

    // ── UploadUserPhotoHandler ────────────────────────────────────────────────

    [Fact]
    public async Task UploadUserPhoto_WhenNotFound_ReturnsNotFound()
    {
        _userRepo.GetByIdAsync(UserId, default).Returns((DomainUser?)null);
        var handler = new UploadUserPhotoHandler(_userRepo, _fileService);

        var result = await handler.Handle(new UploadUserPhotoCommand(UserId, Stream.Null, "photo.jpg", 100), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainUserErrors.NotFound.Code);
    }

    [Fact]
    public async Task UploadUserPhoto_WhenValid_SavesFileAndUpdatesUser()
    {
        var user = MakeUser();
        _userRepo.GetByIdAsync(UserId, default).Returns(user);
        _fileService.SaveUserPhotoAsync(UserId, Arg.Any<Stream>(), "photo.jpg", default).Returns("uploads/users/photo.jpg");
        var handler = new UploadUserPhotoHandler(_userRepo, _fileService);

        var result = await handler.Handle(new UploadUserPhotoCommand(UserId, Stream.Null, "photo.jpg", 100), default);

        result.IsError.Should().BeFalse();
        user.PhotoPath.Should().Be("uploads/users/photo.jpg");
        await _userRepo.Received(1).UpdateAsync(user, default);
    }

    [Fact]
    public async Task UploadUserPhoto_WhenExistingPhoto_DeletesOldFile()
    {
        var user = MakeUser();
        user.ReplacePhoto("old/path.jpg");
        _userRepo.GetByIdAsync(UserId, default).Returns(user);
        _fileService.SaveUserPhotoAsync(UserId, Arg.Any<Stream>(), "photo.jpg", default).Returns("new/path.jpg");
        var handler = new UploadUserPhotoHandler(_userRepo, _fileService);

        await handler.Handle(new UploadUserPhotoCommand(UserId, Stream.Null, "photo.jpg", 100), default);

        await _fileService.Received(1).DeleteFileAsync("old/path.jpg", default);
    }

    // ── DeleteUserPhotoHandler ────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserPhoto_WhenNotFound_ReturnsNotFound()
    {
        _userRepo.GetByIdAsync(UserId, default).Returns((DomainUser?)null);
        var handler = new DeleteUserPhotoHandler(_userRepo, _fileService);

        var result = await handler.Handle(new DeleteUserPhotoCommand(UserId), default);

        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be(DomainUserErrors.NotFound.Code);
    }

    [Fact]
    public async Task DeleteUserPhoto_WhenPhotoExists_DeletesFileAndClears()
    {
        var user = MakeUser();
        user.ReplacePhoto("uploads/users/photo.jpg");
        _userRepo.GetByIdAsync(UserId, default).Returns(user);
        var handler = new DeleteUserPhotoHandler(_userRepo, _fileService);

        var result = await handler.Handle(new DeleteUserPhotoCommand(UserId), default);

        result.IsError.Should().BeFalse();
        user.PhotoPath.Should().BeNull();
        await _fileService.Received(1).DeleteFileAsync("uploads/users/photo.jpg", default);
        await _userRepo.Received(1).UpdateAsync(user, default);
    }

    [Fact]
    public async Task DeleteUserPhoto_WhenNoPhoto_DoesNotCallDeleteFile()
    {
        var user = MakeUser();
        _userRepo.GetByIdAsync(UserId, default).Returns(user);
        var handler = new DeleteUserPhotoHandler(_userRepo, _fileService);

        var result = await handler.Handle(new DeleteUserPhotoCommand(UserId), default);

        result.IsError.Should().BeFalse();
        await _fileService.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), default);
    }
}


