namespace Application.DTOs.Request;

public sealed record EmbeddingRequest(string Model, string Prompt);