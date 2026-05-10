namespace WorkTogetherly.Infrastructure.Settings;

public class ResendSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string FromAddress { get; init; } = "noreply@worktogetherly.com";
}
