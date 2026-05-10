namespace WorkTogetherly.Presentation.Models.Auth
{
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
