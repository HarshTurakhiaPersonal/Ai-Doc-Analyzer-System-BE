namespace Application.DTOs.Request;

public sealed record OllamaGenerateRequest(string Model, string Prompt, string keep_alive, bool Stream = false);

public sealed record OllamaMessage(string Role, string Content);