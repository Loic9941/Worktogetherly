namespace WorkTogetherly.Presentation.Models.Auth;

public record ResetPasswordRequest(string Email, string Token, string NewPassword);
