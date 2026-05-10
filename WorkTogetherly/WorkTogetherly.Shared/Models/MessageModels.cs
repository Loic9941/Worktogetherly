namespace WorkTogetherly.Shared.Models;

public record MessageDto(
    int Id,
    string Content,
    bool IsRead,
    DateTime CreatedAt);
