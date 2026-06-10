namespace Application.DTOs.Response;

public sealed record EmbeddingResponse(float[] Embedding);

public sealed record OllamaEmbeddingResponse(float[] Embedding);