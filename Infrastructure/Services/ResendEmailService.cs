using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using WorkTogetherly.Application.Interfaces;
using WorkTogetherly.Infrastructure.Settings;

namespace WorkTogetherly.Infrastructure.Services;

public class ResendEmailService(IResend resend, IOptions<ResendSettings> options, ILogger<ResendEmailService> logger)
    : IEmailService
{
    private readonly ResendSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            var message = new EmailMessage
            {
                From = "do-not-reply@worktogetherly.be",
                Subject = subject,
                HtmlBody = htmlBody,
            };
            message.To.Add(to);

            await resend.EmailSendAsync(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", to, subject);
        }
    }
}
