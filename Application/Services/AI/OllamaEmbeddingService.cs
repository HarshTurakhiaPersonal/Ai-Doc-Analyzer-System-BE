using Application.DTOs.Response;
using Application.Interfaces;
using Microsoft.Extensions.Options;
using Pgvector;
using Shared.Options;
using System.Net.Http.Json;

namespace Application.Services.AI;

public sealed class OllamaEmbeddingService(IHttpClientFactory httpClientFactory, IOptions<OllamaOptions> options) : IEmbeddingService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("Ollama");

    public async Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            model = options.Value.EmbeddingModel,
            prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync(
                "/api/embeddings",
                request,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: cancellationToken);

        return new Vector(result!.Embedding);
    }
}