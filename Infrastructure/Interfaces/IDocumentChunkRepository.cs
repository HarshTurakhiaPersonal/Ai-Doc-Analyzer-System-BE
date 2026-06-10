using Domain.Entities;
using Pgvector;

namespace Application.Interfaces.IRepositories;

public interface IDocumentChunkRepository
{
    Task AddRangeAsync(IEnumerable<DocumentChunk> chunks);

    Task<List<DocumentChunk>> SearchSimilarChunksAsync(Guid documentId, Vector embedding, int topK, CancellationToken cancellationToken = default);

    Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);

    Task<List<DocumentChunk>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}