using Application.DTOs.Request;
using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Options;
using System.Net.Http.Json;

namespace Application.Services.AI;

public sealed class ChatService(
    IHttpClientFactory httpClientFactory,
    ILogger<ChatService> logger,
    IOptions<OllamaOptions> options) : IOllamaChatService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Ollama");

    public async Task<string> AskAsync(string prompt, CancellationToken cancellationToken = default)
    {
        OllamaGenerateRequest request = new(
            options.Value.ChatModel,
            prompt,
            keep_alive: "30m",
            false
            );

        var response = await _httpClient.PostAsJsonAsync("/api/generate", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(cancellationToken: cancellationToken);

        return result?.Response?.Trim() ?? string.Empty;
    }
}