namespace WorkTogetherly.Application.Messages.Common;

public record MessageResult(
    int Id,
    string Content,
    bool IsRead,
    DateTime CreatedAt);
