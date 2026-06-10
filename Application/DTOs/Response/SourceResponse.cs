namespace Application.DTOs.Response;

public sealed record SourceResponse(
    int ChunkIndex,
    string Content);