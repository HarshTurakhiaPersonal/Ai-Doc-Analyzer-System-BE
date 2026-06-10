namespace Application.DTOs.Response;

public sealed record ChatSessionResponse(
    Guid Id,
    Guid DocumentId,
    string Title,
    DateTime LastMessageAt);