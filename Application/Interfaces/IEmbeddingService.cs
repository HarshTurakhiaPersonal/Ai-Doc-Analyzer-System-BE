using Pgvector;

namespace Application.Interfaces;

public interface IEmbeddingService
{
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}