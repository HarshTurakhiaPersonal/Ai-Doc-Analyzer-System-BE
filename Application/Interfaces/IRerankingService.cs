using Domain.Entities;

namespace Application.Interfaces;

public interface IRerankingService
{
    Task<List<DocumentChunk>> RerankAsync(string question, List<DocumentChunk> chunks);
}
