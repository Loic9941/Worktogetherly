namespace WorkTogetherly.Shared.Models
{
    public record UserProfileDto(Guid Id, string Email, string FirstName, string LastName, string? PhotoPath = null);

    public record UpdateProfileDto(string FirstName, string LastName);
}
