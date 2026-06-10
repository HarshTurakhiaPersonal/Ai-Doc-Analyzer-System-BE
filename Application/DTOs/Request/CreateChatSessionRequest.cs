namespace Application.DTOs.Request;

public sealed record CreateChatSessionRequest(
    Guid DocumentId,
    string? Title);