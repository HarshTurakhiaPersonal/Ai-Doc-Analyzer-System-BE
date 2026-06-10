namespace Application.DTOs.Response;

public sealed record ChatMessageResponse(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt);