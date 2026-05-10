using WorkTogetherly.Application.Contracts;

namespace WorkTogetherly.Application.Services;

public record RegisterResult(bool Success, string[] Errors, UserDto? User = null);
