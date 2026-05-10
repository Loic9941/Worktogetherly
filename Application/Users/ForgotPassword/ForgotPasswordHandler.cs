using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Application.Settings;
using WorkTogetherly.Domain.Interfaces;

namespace WorkTogetherly.Application.Users.ForgotPassword;

public class ForgotPasswordHandler(
    IUserRepository userRepository,
    IEmailService emailService,
    IOptions<FrontendSettings> frontendOptions) : IRequestHandler<ForgotPasswordCommand, ErrorOr<Success>>
{
    private readonly FrontendSettings _frontend = frontendOptions.Value;

    public async Task<ErrorOr<Success>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            return Result.Success;

        var token = await userRepository.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var encodedEmail = Uri.EscapeDataString(request.Email);
        var link = $"{_frontend.BaseUrl}/reset-password?token={encodedToken}&email={encodedEmail}";

        await emailService.SendAsync(
            user.Email!,
            "Réinitialisation de votre mot de passe — WorkTogetherly",
            $"""
            <p>Bonjour {user.FirstName},</p>
            <p>Vous avez demandé la réinitialisation de votre mot de passe WorkTogetherly.</p>
            <p><a href="{link}">Cliquez ici pour choisir un nouveau mot de passe</a></p>
            <p>Ce lien est valide 24 heures. Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.</p>
            """,
            cancellationToken);

        return Result.Success;
    }
}
