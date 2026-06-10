using Application.Interfaces.IRepositories;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace Infrastructure.Repositories;

public sealed class DocumentChunkRepository(AppDbContext context) : IDocumentChunkRepository
{
    public async Task AddRangeAsync(IEnumerable<DocumentChunk> chunks)
    {
        await context.DocumentChunks.AddRangeAsync(chunks);
    }

    public async Task<List<DocumentChunk>> SearchSimilarChunksAsync(Guid documentId, Vector embedding, int topK, CancellationToken cancellationToken)
    {
        return await context.DocumentChunks
            .FromSqlInterpolated($@"
                SELECT *
                FROM document_chunks
                WHERE ""DocumentId"" = {documentId}
                ORDER BY ""Embedding"" <=> {embedding}
                LIMIT {topK}")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    }

    public async Task<List<DocumentChunk>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await context.DocumentChunks
            .Where(x => x.DocumentId == documentId)
            .OrderBy(x => x.ChunkIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentChunk>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return await context.DocumentChunks
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}