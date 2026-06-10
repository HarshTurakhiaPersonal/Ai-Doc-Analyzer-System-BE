using Pgvector;

namespace Domain.Entities;

public sealed class DocumentChunk : BaseEntity
{
    public Guid DocumentId { get; set; }

    public string ContentText { get; set; } = string.Empty;

    public int ChunkIndex { get; set; }

    public Vector? Embedding { get; set; }

    public Documents Document { get; set; } = null!;
}