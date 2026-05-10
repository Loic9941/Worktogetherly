namespace WorkTogetherly.Application.Contracts;

public record RegisterRequestDto(string? FirstName, string? LastName, string Email, string Password);
