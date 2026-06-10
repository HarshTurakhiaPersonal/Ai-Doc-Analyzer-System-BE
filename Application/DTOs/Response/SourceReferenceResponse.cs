namespace Application.DTOs.Response;

public sealed record SourceReferenceResponse(
    Guid ChunkId,
    int ChunkIndex);
